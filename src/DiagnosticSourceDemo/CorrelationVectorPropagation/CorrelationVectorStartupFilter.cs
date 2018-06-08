using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

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
                next(app);
            };
        }
    }
}
