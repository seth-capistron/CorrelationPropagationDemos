using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CorrelationPropagationDemos.WebApiDemo
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> SendAsync(
            this HttpClient httpClient,
            CorrelationVector correlationVector,
            HttpRequestMessage requestMessage )
        {
            requestMessage.Properties.Add(
                CorrelationVectorPropagationDelegates.HttpRequestMessagePropertyName,
                correlationVector );

            return httpClient.SendAsync( requestMessage );
        }
    }
}
