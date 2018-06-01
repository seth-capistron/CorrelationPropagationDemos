using System.Diagnostics;

namespace CorrelationPropagationDemos.WebApiDemo
{
    public class CorrelationVectorExtension : ActivityExtension
    {
        private string _externalCorrelationVectorParent;

        public CorrelationVectorExtension( Activity activity )
            : base( activity )
        { }

        public CorrelationVector CorrelationVector { get; private set; }

        public void SetExternalCorrelationVectorParent( string correlationVector )
        {
            _externalCorrelationVectorParent = correlationVector;
        }

        public override void ActivityStarted()
        {
            CorrelationVectorExtension parentExtension =
                Activity.Parent?.GetActivityExtension<CorrelationVectorExtension>();

            if ( !string.IsNullOrEmpty( _externalCorrelationVectorParent ) )
            {
                this.CorrelationVector = CorrelationVector.Extend( _externalCorrelationVectorParent );
            }
            else if ( parentExtension != null )
            {
                this.CorrelationVector = parentExtension.CorrelationVector;
            }
            else
            {
                this.CorrelationVector = new CorrelationVector();
            }
        }

        public override void ActivityStopped()
        { }
    }
}
