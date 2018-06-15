using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CorrelationVectorPropagation;
using CorrelationVectorPropagation.AspNetClassic;

namespace OwinSelfHostApplication
{
    public class ValuesController : ApiController
    {
        // GET api/values 
        public async Task<IEnumerable<string>> GetAsync()
        {
            // The incoming isn't getting caught by the Diagnostic Listener - how do
            // we capture it?

            using (HttpClient client = new HttpClient(new DependencyClientHandler("ConsumerStore", "WebService")))
            {
                // Issue two request synchronously
                //
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://consumerstorefd.corp.microsoft.com/health/keepalive");

                // Pass dependency info along to a SendAsync extension method so the info 
                // is stamped on the request and it can be used for logging.
                //
                HttpResponseMessage response = await client.SendAsync(request, "Get_Values", "KeepAlive", "v1.0");
            }

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5 
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values 
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5 
        public void Delete(int id)
        {
        }
    }
}