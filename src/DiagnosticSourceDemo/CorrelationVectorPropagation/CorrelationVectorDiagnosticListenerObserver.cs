using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace CorrelationVectorPropagation
{
    public class CorrelationVectorDiagnosticListenerObserver : IObserver<DiagnosticListener>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CorrelationVectorDiagnosticListenerObserver(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        private class CorrelationVectorDiagnosticSourceWriteObserver : IObserver<KeyValuePair<string, object>>
        {
            private IHttpContextAccessor httpContextAccessor;

            public CorrelationVectorDiagnosticSourceWriteObserver(IHttpContextAccessor httpContextAccessor)
            {
                this.httpContextAccessor = httpContextAccessor;
            }

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

                    if (httpContext.Request.Headers.ContainsKey("MS-CV"))
                    {
                        httpContext.SetCorrelationVector(
                            CorrelationVector.Extend(httpContext.Request.Headers["MS-CV"][0]));
                    }
                    else
                    {
                        httpContext.Items.Add(typeof(CorrelationVector), new CorrelationVector());
                    }
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

                    // If the application code explicitly passed along the cV header from the incoming request, then increment it prior to the outbound request.
                    CorrelationVector correlationVector = requestMessage.GetCorrelationVector();
                    if (correlationVector != null)
                    {
                        requestMessage.Headers.Add("MS-CV", correlationVector.Increment());
                        // TODO: Set the incremented cV back on the HttpContext for subsequent requests?
                    }
                    else
                    {
                        // This is the expected case where the application code is unaware of the cV and did not get it from the incoming request and set it on the 
                        // outgoing request. Get the current cV from the HttpContext, increment it, and add it to the outgoing request.
                        correlationVector = this.httpContextAccessor.HttpContext.GetCorrelationVector();
                        if (correlationVector != null)
                        {
                            requestMessage.Headers.Add("MS-CV", correlationVector.Increment());
                            // TODO: Set the incremented cV back on the HttpContext for subsequent requests?
                        }
                        else
                        {
                            // TODO: Should never hit this case. Just in case we do, initialize a new cV here.
                        }
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
                value.Subscribe(new CorrelationVectorDiagnosticSourceWriteObserver(this.httpContextAccessor));
            }
        }
    }
}
