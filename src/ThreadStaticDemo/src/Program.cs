using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace CorrelationPropagationDemos.ThreadStaticDemo
{
    public class Program
    {
        public static void Main( string[] args )
        {
            // Issue a request to the controller to simulate a caller
            //
            Timer executeCallTimer = new Timer( IssueRequest, state: null, dueTime: 1500, period: Timeout.Infinite );
            
            using ( DiagnosticListener.AllListeners.Subscribe( new CorrelationVectorDiagnosticListenerObserver() ) )
            {
                CreateWebHostBuilder( args ).Build().Run();
            }
        }

        static void IssueRequest( object state )
        {
            using ( HttpClient client = new HttpClient() )
            {
                var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost:5002/api/values" );

                // Include an MS-CV in the request to the controller to make sure
                // this value gets picked up and appropriately extended
                //
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
