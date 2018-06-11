using System.Net.Http;
using System.Threading.Tasks;

namespace CorrelationVectorPropagation
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> SendAsync(
            this HttpClient httpClient,
            HttpRequestMessage requestMessage,
            string operationName,
            string dependencyOperationName,
            string dependencyOperationVersion )
        {
            // Add this dependency information to the request so it can be used for logging
            //
            requestMessage.Properties.Add(
                nameof(DependencyOperationInfo),
                new DependencyOperationInfo()
                {
                    OperationName = operationName,
                    DepenedencyOperationName = dependencyOperationName,
                    DependencyOperationVersion = dependencyOperationVersion
                });

            return await httpClient.SendAsync(requestMessage);
        }
    }
}
