using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace CorrelationPropagationDemos.DiagnosticSourceDemo
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
                    HttpContext httpContext =
                        value.Value.GetType().GetProperty( "HttpContext" )?.GetValue( value.Value, null )
                        as HttpContext;

                    if ( httpContext == null )
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
                    HttpRequestMessage requestMessage =
                        value.Value.GetType().GetProperty( "Request" )?.GetValue( value.Value, null )
                        as HttpRequestMessage;

                    if ( requestMessage == null )
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
