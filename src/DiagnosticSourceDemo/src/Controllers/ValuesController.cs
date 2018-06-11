using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CorrelationVectorPropagation;
using Microsoft.AspNetCore.Mvc;

namespace CorrelationPropagationDemos.DiagnosticSourceDemo
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        static List<string> s_outgoingCorrelationVectors = new List<string>();

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KeyValuePair<string, string>>>> Get()
        {
            string incomingRequestCv = TryGetIncomingHeaderValue("MS-CV");
            string incomingCv = CorrelationVector.Current?.Value ?? "<not set>";
            string outgoingCv1, outgoingCv2;

            using (HttpClient client = new HttpClient())
            {
                // Issue two request synchronously
                //
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://consumerstorefd.corp.microsoft.com/health/keepalive");

                HttpResponseMessage response = await client.SendAsync(request);

                outgoingCv1 = TryGetHeaderValue(response.RequestMessage.Headers, "MS-CV");

                request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://consumerstorefd.corp.microsoft.com/health/keepalive");

                response = await client.SendAsync(request);

                outgoingCv2 = TryGetHeaderValue(response.RequestMessage.Headers, "MS-CV");
            }

            // Issue a handful of requests on child threads to make sure they can have distinct CVs
            //
            List<Task> theTasks = new List<Task>();
            s_outgoingCorrelationVectors.Clear();

            for (int i = 0; i < 15; i++)
            {
                theTasks.Add(Task.Run( () => IssueRequest(CorrelationVector.Current.Increment())));
            }

            Task.WaitAll(theTasks.ToArray());

            int matchingOutgoingCorrelationVectors = s_outgoingCorrelationVectors.GroupBy(cv => cv)
                .Where(group => group.Count() > 1)
                .Count();

            // Return a payload that will give us a hint as to the values of the CV throughout the pipe
            //
            return
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("On the incoming request to the controller - Correlation Vector", incomingRequestCv),
                    new KeyValuePair<string, string>("On the AsyncLocal Instance populated before controller code - Correlation Vector", incomingCv),
                    new KeyValuePair<string, string>("On the first outgoing request that the controller sent - Correlation Vector", outgoingCv1),
                    new KeyValuePair<string, string>("On the second outgoing request that the controller sent - Correlation Vector", outgoingCv2),
                    new KeyValuePair<string, string>("Number of parallel outgoing requests", s_outgoingCorrelationVectors.Count().ToString()),
                    new KeyValuePair<string, string>("Number of duplicate outgoing Correlation Vectors", matchingOutgoingCorrelationVectors.ToString()),
                    new KeyValuePair<string, string>("Example of one of the parallel outgoing request - Correlation Vector", s_outgoingCorrelationVectors.Last()),
                };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private void IssueRequest(string correlationVector)
        {
            // Override the AsyncLocal Instance that flowed from the parent thread
            //
            CorrelationVector.Current = CorrelationVector.Extend(correlationVector);

            var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://consumerstorefd.corp.microsoft.com/health/keepalive" );

            using (HttpClient client = new HttpClient())
            {
                var response = client.SendAsync(request).Result;

                s_outgoingCorrelationVectors.Add(TryGetHeaderValue(response.RequestMessage.Headers, "MS-CV"));
            }
        }

        string TryGetIncomingHeaderValue(string headerName)
        {
            return Request.Headers.ContainsKey(headerName) ? Request.Headers[headerName][0] : "<not set>";
        }

        string TryGetHeaderValue(HttpRequestHeaders headers, string headerName)
        {
            if (!headers.Contains(headerName))
            {
                return "<not set>";
            }
            else
            {
                return string.Join(
                    ',',
                    headers.GetValues(headerName));
            }
        }
    }
}
