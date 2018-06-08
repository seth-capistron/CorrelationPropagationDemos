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
            public void OnCompleted()
            { }

            public void OnError( Exception error )
            { }

            public void OnNext( KeyValuePair<string, object> value )
            {
                if ( value.Key == "Microsoft.AspNetCore.Hosting.HttpRequestIn.Start" )
                {
                    // This happens on incoming requests to ASP.NET. Grab the MS-CV header and store
                    // it on the HttpContext.
                    //
                    if (!(value.Value.GetType().GetProperty("HttpContext")?.GetValue(value.Value, null) is HttpContext httpContext))
                    {
                        return;
                    }

                    if ( httpContext.Request.Headers.ContainsKey( "MS-CV" ) )
                    {
                        httpContext.SetCorrelationVector(
                            CorrelationVector.Extend( httpContext.Request.Headers["MS-CV"][0] ) );
                    }
                    else
                    {
                        httpContext.Items.Add( typeof( CorrelationVector ), new CorrelationVector() );
                    }
                }
                else if ( value.Key == "System.Net.Http.HttpRequestOut.Start" )
                {
                    // This happens on outgoing Http requests via Http Client. See if a Correlation
                    // Vector has been stored on the Request Message's properties and use it to
                    // stamp an MS-CV header (after incrementing the CV).
                    //
                    if (!(value.Value.GetType().GetProperty("Request")?.GetValue(value.Value, null) is HttpRequestMessage requestMessage))
                    {
                        return;
                    }

                    CorrelationVector correlationVector = requestMessage.GetCorrelationVector();

                    if ( correlationVector != null )
                    {
                        requestMessage.Headers.Add( "MS-CV", correlationVector.Increment() );
                    }
                }
            }
        }

        public void OnCompleted()
        { }

        public void OnError( Exception error )
        { }

        public void OnNext( DiagnosticListener value )
        {
            if ( value.Name.Equals( "HttpHandlerDiagnosticListener" ) ||
                value.Name.Equals( "Microsoft.AspNetCore" ) )
            {
                value.Subscribe( new CorrelationVectorDiagnosticSourceWriteObserver() );
            }
        }
    }
}
