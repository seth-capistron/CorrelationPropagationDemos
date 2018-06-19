using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

namespace CorrelationVectorPropagation
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

        public static string GetCorrelationVectorHeader( this HttpRequestMessage requestMessage )
        {
            IEnumerable<string> cvValues;

            if ( requestMessage.Headers.TryGetValues( CorrelationVector.HeaderName, out cvValues ) )
            {
                return cvValues.First();
            }
            else
            {
                return null;
            }
        }
    }
}
