using System;
using System.Diagnostics;
using System.Net.Http;
using CorrelationVectorPropagation.AspNetClassic;
using Microsoft.Owin.Hosting;

namespace OwinSelfHostApplication
{
    class Program
    {
        static void Main()
        {
            // Register the Diagnostic Listener Observer, which will consume and propagate CVs and
            // can also do logging
            //
            DiagnosticListener.AllListeners.Subscribe(new CorrelationVectorDiagnosticListenerObserver());

            string baseAddress = "http://localhost:9000/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                // Create HttpCient and make a request to api/values 
                //
                HttpClient client = new HttpClient();
                
                var response = client.GetAsync(baseAddress + "api/values").Result;

                Console.ReadLine();
            }
        }
    }
}
