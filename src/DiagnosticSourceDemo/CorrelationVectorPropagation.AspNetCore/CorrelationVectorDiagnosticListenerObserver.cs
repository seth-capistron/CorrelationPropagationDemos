using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace CorrelationVectorPropagation
{
    public class CorrelationVectorDiagnosticListenerObserver : IObserver<DiagnosticListener>
    {
        private class CorrelationVectorDiagnosticSourceWriteObserver : IObserver<KeyValuePair<string, object>>
        {
            private object _outgoingRequestLoggingLock = new object();

            public void OnCompleted()
            { }

            public void OnError(Exception error)
            { }

            public void OnNext(KeyValuePair<string, object> value)
            {
                if (value.Key == "Microsoft.AspNetCore.Hosting.HttpRequestIn.Start")
                {
                    // This happens on incoming requests to ASP.NET. Grab the MS-CV header and store
                    // it on the HttpContext.
                    //
                    if (!(value.Value.GetType().GetProperty("HttpContext")?.GetValue(value.Value, null) is HttpContext httpContext))
                    {
                        return;
                    }

                    CorrelationVector correlationVector;

                    if (httpContext.Request.Headers.ContainsKey("MS-CV"))
                    {
                        correlationVector =
                            CorrelationVector.Extend(httpContext.Request.Headers["MS-CV"][0]);
                    }
                    else
                    {
                        correlationVector = new CorrelationVector();
                    }

                    CorrelationVector.Current = correlationVector;
                }
                else if (value.Key == "System.Net.Http.HttpRequestOut.Start")
                {
                    // This happens on outgoing Http requests via Http Client. See if a Correlation
                    // Vector has been stored on the Request Message's properties and use it to
                    // stamp an MS-CV header (after incrementing the CV).
                    //
                    if (!(value.Value.GetType().GetProperty("Request")?.GetValue(value.Value, null) is HttpRequestMessage requestMessage))
                    {
                        return;
                    }

                    requestMessage.Properties.Add("TimeStamp", DateTime.Now.Ticks);

                    // If the application code explicitly passed along the cV header from the incoming request, then increment it prior to the outbound request.
                    CorrelationVector correlationVector = requestMessage.GetCorrelationVector();
                    if (correlationVector != null)
                    {
                        requestMessage.Headers.Add("MS-CV", correlationVector.Increment());
                    }
                    else
                    {
                        // This is the expected case where the application code is unaware of the cV and did not get it from the incoming request and set it on the 
                        // outgoing request. Get the current cV from the AsyncLocal Instance, increment it, and add it to the outgoing request.
                        correlationVector = CorrelationVector.Current;
                        if (correlationVector != null)
                        {
                            requestMessage.Headers.Add("MS-CV", correlationVector.Increment());
                        }
                        else
                        {
                            // TODO: Should never hit this case. Just in case we do, initialize a new cV here.
                        }
                    }
                }
                else if (value.Key == "System.Net.Http.HttpRequestOut.Stop")
                {
                    // This happens after an outgoing Http request via Http Client completes. We can use
                    // this as an opportunity to log the Outgoing Service Request event. Many of the
                    // properties used for logging can be gleaned from the Http request and response.
                    // Some of the properties need to be stamped on the request ahead of time by the
                    // service owner, such as the Dependency information.
                    //
                    if (!(value.Value.GetType().GetProperty("Response")?.GetValue(value.Value, null) is HttpResponseMessage responseMessage))
                    {
                        return;
                    }

                    long? latency = null;

                    if (responseMessage.RequestMessage.Properties.ContainsKey("TimeStamp") &&
                        responseMessage.RequestMessage.Properties["TimeStamp"] is long requestTimeStamp)
                    {
                        latency = (long)(DateTime.Now - new DateTime(requestTimeStamp)).TotalMilliseconds;
                    }

                    // Acquire a lock just so the console output isn't jumbled up by multiple threads
                    //
                    lock (_outgoingRequestLoggingLock)
                    {
                        // These properties can be gleaned from the Http request and response
                        //
                        Console.WriteLine("---------------------------------------");
                        Console.WriteLine("Logging the Outgoing Service Request...");
                        Console.WriteLine("Correlation Vector: {0}", responseMessage.RequestMessage.GetCorrelationVectorHeader());
                        Console.WriteLine("Target Uri: {0}", responseMessage.RequestMessage.RequestUri.ToString());
                        Console.WriteLine("Latency ms: {0}", latency.HasValue ? latency.Value.ToString() : string.Empty);
                        // TODO - need code to be able to read the service error code - should this code be common or customizable?
                        Console.WriteLine("Service error code: {0}", null);
                        Console.WriteLine("Succeeded: {0}", responseMessage.IsSuccessStatusCode);
                        Console.WriteLine("Request method: {0}", responseMessage.RequestMessage.Method);
                        Console.WriteLine("Protocol Status Code: {0}", (int)responseMessage.StatusCode);
                        Console.WriteLine("Response Size (bytes): {0}", responseMessage.Content.ReadAsByteArrayAsync().Result.Length);

                        // These properties need to be stamped on the request by the service owner
                        // through the use of DependencyClientHandler and a HttpClient.SendAsync
                        // extension method.
                        //
                        if (responseMessage.RequestMessage.Properties.ContainsKey(nameof(DependencyOperationInfo)) &&
                            responseMessage.RequestMessage.Properties[nameof(DependencyOperationInfo)] is DependencyOperationInfo dependencyInfo)
                        {
                            Console.WriteLine("Dependency Name: {0}", dependencyInfo.DependencyName);
                            Console.WriteLine("Dependency Type: {0}", dependencyInfo.DependencyType);
                            Console.WriteLine("Operation Name: {0}", dependencyInfo.OperationName);
                            Console.WriteLine("Dependency Operation Name: {0}", dependencyInfo.DependencyOperationName);
                            Console.WriteLine("Dependency Operation Version: {0}", dependencyInfo.DependencyOperationVersion);
                        }

                        Console.WriteLine("---------------------------------------");
                    }
                }
            }
        }

        public void OnCompleted()
        { }

        public void OnError(Exception error)
        { }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name.Equals("HttpHandlerDiagnosticListener") ||
                value.Name.Equals("Microsoft.AspNetCore"))
            {
                value.Subscribe(new CorrelationVectorDiagnosticSourceWriteObserver());
            }
        }
    }
}
