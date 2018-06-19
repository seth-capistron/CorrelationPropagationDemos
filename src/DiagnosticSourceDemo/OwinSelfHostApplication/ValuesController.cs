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
            using (HttpClient client = new HttpClient(new DependencyClientHandler("ConsumerStore", "WebService")))
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://consumerstorefd.corp.microsoft.com/health/keepalive");

                HttpResponseMessage response = await client.SendAsync(
                    request, "Get_Values", "KeepAlive", "1.0");
            }

            using (HttpClient client = new HttpClient(new DependencyClientHandler("DisplayCatalog", "WebService")))
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://displaycatalog.mp.microsoft.com/v7.0/products/9NKSQGP7F2NH?fieldsTemplate=details&market=US&languages=en-us&catalogId=1");

                HttpResponseMessage response = await client.SendAsync(
                    request, "Get_Values", "Hydrate", "7.0");
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