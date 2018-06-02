using System.Net.Http;

namespace CorrelationPropagationDemos.DiagnosticSourceDemo
{
    public static class HttpRequestMessageExtensions
    {
        public static void AddCorrelationVector(this HttpRequestMessage requestMessage, CorrelationVector correlationVector )
        {
            requestMessage.Properties.Add( nameof( CorrelationVector ), correlationVector );
        }

        public static CorrelationVector GetCorrelationVector( this HttpRequestMessage requestMessage )
        {
            if ( requestMessage.Properties.ContainsKey( nameof( CorrelationVector ) ) )
            {
                return requestMessage.Properties[nameof( CorrelationVector )] as CorrelationVector;
            }
            else
            {
                return null;
            }
        }
    }
}
