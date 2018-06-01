using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;

namespace CorrelationPropagationDemos.WebApiDemo
{
    public class CorrelationVectorConsumer : ICorrelationConsumer
    {
        public void BeginRequest( HttpContext httpContext, HostingApplication.Context context )
        {
            var currentExtension = context.Activity?.GetActivityExtension<CorrelationVectorExtension>();

            if ( currentExtension != null &&
                httpContext.Request.Headers.ContainsKey( CorrelationVectorPropagationDelegates.HeaderName ) )
            {
                currentExtension.SetExternalCorrelationVectorParent(
                    httpContext.Request.Headers[CorrelationVectorPropagationDelegates.HeaderName][0] );
            }
        }
    }
}
