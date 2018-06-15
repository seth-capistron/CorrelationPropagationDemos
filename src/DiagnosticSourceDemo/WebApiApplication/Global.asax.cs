using System.Diagnostics;
using System.Web.Http;
using CorrelationVectorPropagation.AspNetClassic;

namespace WebApiApplication
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Register the Diagnostic Listener Observer, which will consume and propagate CVs and
            // can also do logging
            //
            DiagnosticListener.AllListeners.Subscribe(new CorrelationVectorDiagnosticListenerObserver());
        }
    }
}
