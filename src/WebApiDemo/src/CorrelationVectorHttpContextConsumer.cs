using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;

namespace CorrelationPropagationDemos.WebApiDemo
{
    public class CorrelationVectorHttpContextConsumer : ICorrelationConsumer
    {
        public void BeginRequest( HttpContext httpContext, HostingApplication.Context context )
        {
            if ( httpContext.Request.Headers.ContainsKey( CorrelationVectorPropagationDelegates.HeaderName ) )
            {
                httpContext.SetCorrelationVector(
                    CorrelationVector.Extend( httpContext.Request.Headers[CorrelationVectorPropagationDelegates.HeaderName][0] ) );
            }
        }
    }
}
