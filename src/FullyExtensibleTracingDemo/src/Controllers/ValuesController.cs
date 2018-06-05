using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CorrelationPropagationDemos.FullyExtensibleTracingDemo.Controllers
{
    [Route( "api/[controller]" )]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KeyValuePair<string, string>>>> Get()
        {
            string incomingRequestActivityId = TryGetIncomingHeaderValue( "Request-Id" );
            string incomingRequestCv = TryGetIncomingHeaderValue( CorrelationVectorPropagationDelegates.HeaderName );
            string incomingActivityId = Activity.Current?.Id ?? "<not set>";
            string incomingCv =
                Activity.Current?.GetActivityExtension<CorrelationVectorExtension>()?.CorrelationVector.Value ?? "<not set>";
            string outgoingActivityId, outgoingCv;

            using ( HttpClient client = new HttpClient() )
            {
                HttpResponseMessage response;
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://consumerstorefd.corp.microsoft.com/health/keepalive" );                

                response = await client.SendAsync( request );
                
                outgoingActivityId = TryGetHeaderValue( response.RequestMessage.Headers, "Request-Id" );
                outgoingCv = TryGetHeaderValue( response.RequestMessage.Headers, CorrelationVectorPropagationDelegates.HeaderName );
            }

            // Return a payload that will give us a hint as to the values of Request-Id and CV throughout the pipe
            //
            return
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("On the incoming request to the controller - Activity Id", incomingRequestActivityId),
                    new KeyValuePair<string, string>("On the incoming request to the controller - Correlation Vector", incomingRequestCv),
                    new KeyValuePair<string, string>("On the Activity populated before controller code - Activity Id", incomingActivityId),
                    new KeyValuePair<string, string>("On the Activity populated before controller code - Correlation Vector", incomingCv),
                    new KeyValuePair<string, string>("On the outgoing request that the controller sent - Activity Id", outgoingActivityId),
                    new KeyValuePair<string, string>("On the outgoing request that the controller sent - Correlation Vector", outgoingCv),
                };
        }

        // GET api/values/5
        [HttpGet( "{id}" )]
        public ActionResult<string> Get( int id )
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post( [FromBody] string value )
        {
        }

        // PUT api/values/5
        [HttpPut( "{id}" )]
        public void Put( int id, [FromBody] string value )
        {
        }

        // DELETE api/values/5
        [HttpDelete( "{id}" )]
        public void Delete( int id )
        {
        }

        string TryGetIncomingHeaderValue( string headerName )
        {
            return Request.Headers.ContainsKey( headerName ) ? Request.Headers[headerName][0] : "<not set>";
        }

        string TryGetHeaderValue( HttpRequestHeaders headers, string headerName )
        {
            if ( !headers.Contains( headerName ) )
            {
                return "<not set>";
            }
            else
            {
                return string.Join(
                    ',',
                    headers.GetValues( headerName ) );
            }
        }
    }
}
