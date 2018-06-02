using System.Net.Http;
using System.Threading.Tasks;

namespace CorrelationPropagationDemos.DiagnosticSourceDemo
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> SendAsync(
            this HttpClient httpClient,
            CorrelationVector correlationVector,
            HttpRequestMessage requestMessage )
        {
            requestMessage.AddCorrelationVector( correlationVector );

            return httpClient.SendAsync( requestMessage );
        }
    }
}
