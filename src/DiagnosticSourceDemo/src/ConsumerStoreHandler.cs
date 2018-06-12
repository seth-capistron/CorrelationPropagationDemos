using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CorrelationVectorPropagation;

namespace CorrelationPropagationDemos.DiagnosticSourceDemo
{
    public class ConsumerStoreHandler : DelegatingHandler
    {
        public ConsumerStoreHandler() : base(new HttpClientHandler())
        { }

        protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
        {
            // Stamp this info on the request so it can be used for logging
            //
            request.AddDependencyInfo("ConsumerStore", "WebService");

            return base.SendAsync( request, cancellationToken );
        }
    }
}
