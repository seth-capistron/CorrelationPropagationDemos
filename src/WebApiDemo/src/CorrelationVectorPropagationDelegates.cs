using System.Diagnostics;
using System.Net.Http;

namespace CorrelationPropagationDemos.WebApiDemo
{
    public static class CorrelationVectorPropagationDelegates
    {
        public const string HeaderName = "MS-CV";
        public const string HttpRequestMessagePropertyName = "CorrelationVector";

        public static void FromCurrentActivityExtension( HttpRequestMessage requestMessage )
        {
            Activity currentActivity = Activity.Current;
            CorrelationVectorExtension currentExtension =
                currentActivity?.GetActivityExtension<CorrelationVectorExtension>();

            if ( currentExtension != null )
            {
                requestMessage.Headers.Add( HeaderName, currentExtension.CorrelationVector.Increment() );
            }
        }

        public static void FromHttpRequestMessageProperty( HttpRequestMessage requestMessage )
        {
            if ( requestMessage.Properties.ContainsKey( HttpRequestMessagePropertyName ) )
            {
                CorrelationVector correlationVector =
                    requestMessage.Properties[HttpRequestMessagePropertyName] as CorrelationVector;

                if ( correlationVector != null )
                {
                    requestMessage.Headers.Add( HeaderName, correlationVector.Increment() );
                }
            }
        }
    }
}
