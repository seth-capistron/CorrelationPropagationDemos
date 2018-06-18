using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CorrelationVectorPropagation.AspNetClassic
{
    /// <summary>
    /// This Delegating Handler is used to hook into the inbound Http Requests to a
    /// self-hosted WebApi application. This is necessary since the usual AspNet events
    /// are not fired for self-hosted WebApi. This enables consumption of incoming
    /// Correlation Vectors.
    /// </summary>
    public class InboundInstrumentationHandler : DelegatingHandler
    {
        const string DiagnosticListenerName = "CorrelationVectorPropagation.AspNetClassic.WebApi.RequestIn";
        const string StartEventName = DiagnosticListenerName + ".Start";
        const string EndEventName = DiagnosticListenerName + ".End";

        private static DiagnosticSource InboundLogger = new DiagnosticListener(DiagnosticListenerName);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (InboundLogger.IsEnabled(StartEventName))
            {
                InboundLogger.Write(StartEventName, new { Request = request });
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (InboundLogger.IsEnabled(EndEventName))
            {
                InboundLogger.Write(EndEventName, new { Response = response });
            }

            return response;
        }
    }
}
