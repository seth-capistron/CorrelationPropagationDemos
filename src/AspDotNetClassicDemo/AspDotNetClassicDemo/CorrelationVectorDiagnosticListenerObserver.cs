using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AspDotNetClassicDemo
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
            }
        }

        public void OnCompleted()
        { }

        public void OnError(Exception error)
        { }

        public void OnNext(DiagnosticListener value)
        {
            value.Subscribe(new CorrelationVectorDiagnosticSourceWriteObserver());
        }
    }
}