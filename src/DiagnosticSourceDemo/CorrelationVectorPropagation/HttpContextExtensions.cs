using Microsoft.AspNetCore.Http;

namespace CorrelationVectorPropagation
{
    public static class HttpContextExtensions
    {
        public static CorrelationVector GetCorrelationVector( this HttpContext httpContext )
        {
            return httpContext.Items[typeof( CorrelationVector )] as CorrelationVector;
        }

        public static void SetCorrelationVector( this HttpContext httpContext, CorrelationVector correlationVector )
        {
            httpContext.Items.Add( typeof( CorrelationVector ), correlationVector );
        }
    }
}
