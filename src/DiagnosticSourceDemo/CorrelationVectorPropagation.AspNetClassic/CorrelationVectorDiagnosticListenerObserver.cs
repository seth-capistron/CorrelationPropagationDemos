using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Web;

namespace CorrelationVectorPropagation.AspNetClassic
{
    public class CorrelationVectorDiagnosticListenerObserver : IObserver<DiagnosticListener>
    {
        private class CorrelationVectorDiagnosticSourceWriteObserver : IObserver<KeyValuePair<string, object>>
        {
            public void OnCompleted()
            { }

            public void OnError(Exception error)
            { }

            public void OnNext(KeyValuePair<string, object> value)
            {
                if (value.Key == "Microsoft.AspNet.HttpReqIn.Start")
                {
                    // This happens on incoming requests to ASP.NET. Grab the MS-CV header and store
                    // it on CorrelationVector.Current and the HttpContext.Current.
                    //
                    string msCvHeader = HttpContext.Current?.Request?.Headers?["MS-CV"]?.ToString();

                    CorrelationVector correlationVector;

                    if (!string.IsNullOrWhiteSpace(msCvHeader))
                    {
                        correlationVector = CorrelationVector.Extend(msCvHeader);
                    }
                    else
                    {
                        correlationVector = new CorrelationVector();
                    }

                    CorrelationVector.Current = correlationVector;
                    HttpContext.Current.Items.Add("CorrelationVector.Current", correlationVector);
                }
                else if (value.Key == "Microsoft.AspNet.HttpReqIn.ActivityLost.Stop")
                {
                    // This is a work-around for a known issue due to hop between managed and
                    // unmanaged code.
                    //
                    CorrelationVector.Current = HttpContext.Current.Items["CorrelationVector.Current"] as CorrelationVector;
                }
                else if (value.Key == "System.Net.Http.Desktop.HttpRequestOut.Start")
                {
                    // This happens on outgoing Http requests via Http Client.
                    //
                    if (!(value.Value.GetType().GetProperty("Request")?.GetValue(value.Value, null) is HttpWebRequest requestMessage))
                    {
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(requestMessage.Headers["MS-CV"]))
                    {
                        // Request already has a CV. This is expected if an HttpClient SendAsync extension method
                        // is used to pass along request dependency info that should be used for logging.
                        //
                        return;
                    }

                    var correlationVector = CorrelationVector.Current;
                    if (correlationVector != null)
                    {
                        requestMessage.Headers.Add("MS-CV", correlationVector.Increment());
                    }
                    else
                    {
                        // TODO: Should never hit this case. If we do, should we create a new CV?
                    }
                }
                else if (value.Key == "System.Net.Http.Desktop.HttpRequestOut.Ex.Stop")
                {
                    if (!(value.Value.GetType().GetProperty("Request")?.GetValue(value.Value, null) is HttpWebRequest requestMessage) ||
                        !(value.Value.GetType().GetProperty("StatusCode")?.GetValue(value.Value, null) is HttpStatusCode responseStatusCode) ||
                        !(value.Value.GetType().GetProperty("Headers")?.GetValue(value.Value, null) is WebHeaderCollection responseHeaders))
                    {
                        return;
                    }

                    LogHttpOutComplete(requestMessage, responseStatusCode, responseHeaders["Content-Length"]);
                }
                else if (value.Key == "System.Net.Http.Desktop.HttpRequestOut.Stop")
                {
                    if (!(value.Value.GetType().GetProperty("Request")?.GetValue(value.Value, null) is HttpWebRequest requestMessage) ||
                        !(value.Value.GetType().GetProperty("Response")?.GetValue(value.Value, null) is HttpWebResponse responseMessage))
                    {
                        return;
                    }

                    LogHttpOutComplete(requestMessage, responseMessage.StatusCode, responseMessage.ContentLength.ToString());
                }
            }

            private void LogHttpOutComplete(HttpWebRequest requestMessage, HttpStatusCode responseStatus, string responseSize)
            {
                long latency = (long)(requestMessage.Date - DateTime.Now).TotalMilliseconds;
                string correlationVector = requestMessage.Headers["MS-CV"];
                DependencyOperationInfo dependencyOperationInfo = null;

                if (!string.IsNullOrWhiteSpace(correlationVector) &&
                    HttpClientExtensions.DependencyInfoCache.ContainsKey(correlationVector))
                {
                    dependencyOperationInfo = HttpClientExtensions.DependencyInfoCache[correlationVector];
                }

                // These properties can be gleaned from the Http request and response
                //
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("Logging the Outgoing Service Request...");
                Console.WriteLine("Correlation Vector: {0}", requestMessage.Headers["MS-CV"]);
                Console.WriteLine("Target Uri: {0}", requestMessage.RequestUri.ToString());
                Console.WriteLine("Latency ms: {0}", latency);
                // TODO - need code to be able to read the service error code - should this code be common or customizable?
                Console.WriteLine("Service error code: {0}", null);
                Console.WriteLine("Succeeded: {0}", (int)responseStatus < 400);
                Console.WriteLine("Request method: {0}", requestMessage.Method);
                Console.WriteLine("Protocol Status Code: {0}", (int)responseStatus);

                Console.WriteLine("Response Size (bytes): {0}", responseSize);

                // These properties need to be stamped on the request by the service owner
                // through the use of DependencyClientHandler and a HttpClient.SendAsync
                // extension method.
                //
                if (dependencyOperationInfo != null)
                {
                    Console.WriteLine("Dependency Name: {0}", dependencyOperationInfo.DependencyName);
                    Console.WriteLine("Dependency Type: {0}", dependencyOperationInfo.DependencyType);
                    Console.WriteLine("Operation Name: {0}", dependencyOperationInfo.OperationName);
                    Console.WriteLine("Dependency Operation Name: {0}", dependencyOperationInfo.DependencyOperationName);
                    Console.WriteLine("Dependency Operation Version: {0}", dependencyOperationInfo.DependencyOperationVersion);
                }

                Console.WriteLine("---------------------------------------");

                if (dependencyOperationInfo != null)
                {
                    HttpClientExtensions.DependencyInfoCache.Remove(correlationVector);
                }
            }
        }

        public void OnCompleted()
        { }

        public void OnError(Exception error)
        { }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == "Microsoft.AspNet.TelemetryCorrelation" ||
                value.Name == "System.Net.Http.Desktop")
            {
                value.Subscribe(new CorrelationVectorDiagnosticSourceWriteObserver());
            }
        }
    }
}
