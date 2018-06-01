using System;

namespace CorrelationPropagationDemos.WebApiDemo
{
    public class CorrelationVector
    {
        private string _baseValue;
        private int _extension = 0;

        public CorrelationVector()
        {
            _baseValue = Guid.NewGuid().ToString().Substring( 0, 13 ).Replace( "-", string.Empty );
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
