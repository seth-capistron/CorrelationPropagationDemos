using System.Net.Http;
using System.Threading.Tasks;

namespace CorrelationVectorPropagation.AspNetCore
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Use this extension method to ensure Outgoing Service Requests get logged
        /// with the correct Dependency Information.
        /// </summary>
        public static async Task<HttpResponseMessage> SendAsync(
            this HttpClient httpClient,
            HttpRequestMessage requestMessage,
            string operationName,
            string dependencyOperationName,
            string dependencyOperationVersion)
        {
            // Add this dependency information to the request so it can be used for logging
            //
            requestMessage.Properties.Add(
                nameof(DependencyOperationInfo),
                new DependencyOperationInfo()
                {
                    OperationName = operationName,
                    DependencyOperationName = dependencyOperationName,
                    DependencyOperationVersion = dependencyOperationVersion
                });

            return await httpClient.SendAsync(requestMessage);
        }
    }
}
