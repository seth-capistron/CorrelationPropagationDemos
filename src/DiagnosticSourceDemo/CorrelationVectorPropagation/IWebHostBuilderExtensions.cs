using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace CorrelationVectorPropagation
{
    public static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseCorrelationVector(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.ConfigureServices(services =>
            {
                // Required for automatic cV propogation
                services.AddHttpContextAccessor();
            });
            DiagnosticListener.AllListeners.Subscribe(new CorrelationVectorDiagnosticListenerObserver());
            return webHostBuilder;
        }
    }
}
