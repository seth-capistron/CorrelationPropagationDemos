using System.Collections.Generic;
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
        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KeyValuePair<string, string>>>> Get()
        {
            string incomingRequestCv = TryGetIncomingHeaderValue("MS-CV");
            string incomingCv = HttpContext.GetCorrelationVector()?.Value;
            string outgoingCv;

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://consumerstorefd.corp.microsoft.com/health/keepalive");

                HttpResponseMessage response = await client.SendAsync(request);

                outgoingCv = TryGetHeaderValue(response.RequestMessage.Headers, "MS-CV");
            }

            // Return a payload that will give us a hint as to the values of the CV throughout the pipe
            //
            return
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("On the incoming request to the controller - Correlation Vector", incomingRequestCv),
                    new KeyValuePair<string, string>("On the HttpContext populated before controller code - Correlation Vector", incomingCv),
                    new KeyValuePair<string, string>("On the outgoing request that the controller sent - Correlation Vector", outgoingCv),
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
