using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CorrelationVectorPropagation.AspNetClassic
{
    public static class HttpClientExtensions
    {
        internal static Dictionary<string, DependencyOperationInfo> DependencyInfoCache =
            new Dictionary<string, DependencyOperationInfo>();

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
            string correlationVector = CorrelationVector.Current?.Increment();

            if (!string.IsNullOrWhiteSpace(correlationVector))
            {
                requestMessage.Headers.Add(CorrelationVector.HeaderName, correlationVector);

                var dependencyOperationInfo = new DependencyOperationInfo()
                {
                    OperationName = operationName,
                    DependencyOperationName = dependencyOperationName,
                    DependencyOperationVersion = dependencyOperationVersion
                };

                // Add the dependency information to the cache so it can be used for logging
                //
                DependencyInfoCache.Add(correlationVector, dependencyOperationInfo);

                // Add the dependency information to the request message properties so the
                // client handler can add the dependency name and type
                //
                requestMessage.Properties.Add(
                    DependencyClientHandler.RequestPropertyKey,
                    dependencyOperationInfo);
            }

            return await httpClient.SendAsync(requestMessage);
        }
    }
}
