using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CorrelationVectorPropagation.AspNetClassic;

namespace MvcWebApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Register the Diagnostic Listener Observer, which will consume and propagate CVs and
            // can also do logging
            //
            DiagnosticListener.AllListeners.Subscribe(new CorrelationVectorDiagnosticListenerObserver());
        }
    }
}
