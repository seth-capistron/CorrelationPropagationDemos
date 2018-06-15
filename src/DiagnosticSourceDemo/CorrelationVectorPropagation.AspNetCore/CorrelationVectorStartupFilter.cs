using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CorrelationVectorPropagation
{
    public class CorrelationVectorStartupFilter : IStartupFilter
    {
        private IHttpContextAccessor httpContextAccessor;

        public CorrelationVectorStartupFilter(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                // Get the singleton instance of the CorrelationVectorDiagnosticListenerObserver and subscribe it
                var observer = app.ApplicationServices.GetService<CorrelationVectorDiagnosticListenerObserver>();
                DiagnosticListener.AllListeners.Subscribe(observer);

                // Add our TelemetryInitializer
                var telemetryInitializer = app.ApplicationServices.GetService<CorrelationVectorTelemetryInitializer>();
                TelemetryConfiguration.Active.TelemetryInitializers.Add(telemetryInitializer);

                next(app);
            };
        }
    }
}
