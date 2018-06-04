using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace CorrelationPropagationDemos.ThreadStaticDemo
{
    internal class CorrelationVectorDiagnosticListenerObserver : IObserver<DiagnosticListener>
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
                    HttpContext httpContext =
                        value.Value.GetType().GetProperty( "HttpContext" )?.GetValue( value.Value, null )
                        as HttpContext;
                    CorrelationVector correlationVector;

                    if ( httpContext == null )
                    {
                        return;
                    }

                    if ( httpContext.Request.Headers.ContainsKey( "MS-CV" ) )
                    {
                        correlationVector = CorrelationVector.Extend( httpContext.Request.Headers["MS-CV"][0] );
                    }
                    else
                    {
                        correlationVector = new CorrelationVector();
                    }

                    CorrelationVector.Current = correlationVector;
                }
                else if ( value.Key == "System.Net.Http.HttpRequestOut.Start" )
                {
                    // This happens on outgoing Http requests via Http Client. See if a Correlation
                    // Vector has been stored on the Request Message's properties and use it to
                    // stamp an MS-CV header (after incrementing the CV).
                    //
                    HttpRequestMessage requestMessage =
                        value.Value.GetType().GetProperty( "Request" )?.GetValue( value.Value, null )
                        as HttpRequestMessage;

                    if ( requestMessage == null )
                    {
                        return;
                    }

                    CorrelationVector correlationVector = requestMessage.GetCorrelationVector() ?? CorrelationVector.Current;

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
