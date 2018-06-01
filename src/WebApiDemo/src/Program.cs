using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;

namespace CorrelationPropagationDemos.WebApiDemo
{
    public class Program
    {
        public static bool UseActivityToPropagateCv = false;

        public static void Main( string[] args )
        {
            FakeDiagnosticListenerObserver diagnosticListenerObserver = null;

            if ( UseActivityToPropagateCv )
            {
                diagnosticListenerObserver = new FakeDiagnosticListenerObserver( kvp => { } );

                // Enable three extension points to enable Correlation Vector tracking along with the
                // Activity and Request-Id
                //
                Activity.RegisterActivityExtension<CorrelationVectorExtension>();
                HttpClientHandler.RegisterCorrelationPropagationDelegate(
                    CorrelationVectorPropagationDelegates.FromCurrentActivityExtension );
                HostingApplication.RegisterCorrelationConsumer( new CorrelationVectorConsumer() );
            }
            else
            {
                HttpClientHandler.RegisterCorrelationPropagationDelegate(
                    CorrelationVectorPropagationDelegates.FromHttpRequestMessageProperty );
                HostingApplication.RegisterCorrelationConsumer( new CorrelationVectorHttpContextConsumer() );
            }

            // Issue a request to the controller to simulate a caller
            //
            Timer executeCallTimer = new Timer( IssueRequest, state: null, dueTime: 1500, period: Timeout.Infinite );

            using ( diagnosticListenerObserver == null ?
                null:
                DiagnosticListener.AllListeners.Subscribe( diagnosticListenerObserver ) )
            {
                CreateWebHostBuilder( args ).Build().Run();
            }
        }

        static void IssueRequest( object state )
        {
            var handler = new HttpClientHandler()
            {
                CorrelationPropagationOverride = (request) => { }
            };

            using ( HttpClient client = new HttpClient( handler ) )
            {
                var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost:5001/api/values" );

                // Include a Request-Id and MS-CV in the request to the controller to make sure
                // these values get picked up an appropriately extended
                //
                request.Headers.Add( "Request-Id", "|9499af08-4dbd803f118d3a73.11" );
                request.Headers.Add( "MS-CV", "3QlpdZEcm1pkH3PsGaXzl3.4" );

                var response = client.SendAsync( request ).Result;

                var responseData = response.Content.ReadAsAsync<IEnumerable<KeyValuePair<string, string>>>().Result;

                Console.WriteLine( "---------------" );

                foreach ( var item in responseData )
                {
                    Console.WriteLine( "{0}: {1}", item.Key, item.Value );
                }
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder( string[] args ) =>
            WebHost.CreateDefaultBuilder( args )
                .UseStartup<Startup>();
    }
}
