using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CorrelationPropagationDemos.WebApiDemo
{
    public sealed class FakeDiagnosticListenerObserver : IObserver<DiagnosticListener>
    {
        private class FakeDiagnosticSourceWriteObserver : IObserver<KeyValuePair<string, object>>
        {
            private readonly Action<KeyValuePair<string, object>> _writeCallback;

            public FakeDiagnosticSourceWriteObserver( Action<KeyValuePair<string, object>> writeCallback )
            {
                _writeCallback = writeCallback;
            }

            public void OnCompleted()
            {
            }

            public void OnError( Exception error )
            {
            }

            public void OnNext( KeyValuePair<string, object> value )
            {
                _writeCallback( value );
            }
        }

        private readonly Action<KeyValuePair<string, object>> _writeCallback;

        private Func<string, object, object, bool> _writeObserverEnabled = ( name, arg1, arg2 ) => true;

        public FakeDiagnosticListenerObserver( Action<KeyValuePair<string, object>> writeCallback )
        {
            _writeCallback = writeCallback;
        }

        public void OnCompleted()
        {
        }

        public void OnError( Exception error )
        {
        }

        public void OnNext( DiagnosticListener value )
        {
            if ( value.Name.Equals( "HttpHandlerDiagnosticListener" ) ||
                value.Name.Equals( "Microsoft.AspNetCore" ) )
            {
                value.Subscribe( new FakeDiagnosticSourceWriteObserver( _writeCallback ), IsEnabled );
            }
        }

        public void Enable()
        {
            _writeObserverEnabled = ( name, arg1, arg2 ) => true;
        }

        public void Enable( Func<string, bool> writeObserverEnabled )
        {
            _writeObserverEnabled = ( name, arg1, arg2 ) => writeObserverEnabled( name );
        }

        public void Enable( Func<string, object, object, bool> writeObserverEnabled )
        {
            _writeObserverEnabled = writeObserverEnabled;
        }

        public void Disable()
        {
            _writeObserverEnabled = ( name, arg1, arg2 ) => false;
        }

        private bool IsEnabled( string s, object arg1, object arg2 )
        {
            return _writeObserverEnabled.Invoke( s, arg1, arg2 );
        }
    }
}
