using System.Web.Http;
using CorrelationVectorPropagation.AspNetClassic;
using Owin;

namespace OwinSelfHostApplication
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Need to hook up this handler so we have a hook for consuming the 
            // incoming Correlation Vector.
            //
            config.MessageHandlers.Add(new InboundInstrumentationHandler());

            appBuilder.UseWebApi(config);
        }
    }
}