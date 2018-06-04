using System;
using System.Threading;

namespace CorrelationPropagationDemos.ThreadStaticDemo
{
    /// <summary>
    /// This is a dummy implementation for demo'ing CV integration with ASP.NET and 
    /// HTTP Client. Actual implementation would be used instead of this one.
    /// </summary>
    public class CorrelationVector
    {
        private string _baseValue;
        private int _extension = 0;

        static AsyncLocal<CorrelationVector> s_current = new AsyncLocal<CorrelationVector>();

        public CorrelationVector()
        {
            _baseValue = Guid.NewGuid().ToString().Substring( 0, 13 ).Replace( "-", string.Empty );
        }

        public static CorrelationVector Current
        {
            get
            {
                return s_current.Value;
            }
            set
            {
                if ( value != null )
                {
                    s_current.Value = value;
                }
            }
        }

        public string Value
        {
            get { return _baseValue + "." + _extension.ToString(); }
        }

        public string Increment()
        {
            _extension++;

            return Value;
        }

        public static CorrelationVector Extend( string correlationVector )
        {
            return new CorrelationVector()
            {
                _baseValue = correlationVector
            };
        }
    }
}
