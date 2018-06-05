using System.Diagnostics;
using System.Net.Http;

namespace CorrelationPropagationDemos.FullyExtensibleTracingDemo
{
    public static class CorrelationVectorPropagationDelegates
    {
        public const string HeaderName = "MS-CV";

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
    }
}
