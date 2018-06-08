using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorrelationVectorPropagation
{
    class CorrelationVectorTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CorrelationVectorTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var requestTelemetry = telemetry as RequestTelemetry;
            // Is this a TrackRequest() ?
            if (requestTelemetry == null) return;

            var context = httpContextAccessor.HttpContext;
            if (context == null) return;

            if (context.Request != null &&
                !string.IsNullOrWhiteSpace(context.Request.Headers["MS-CV"]))
            {
                requestTelemetry.Context.Properties["MS-CV"] = context.Request.Headers["MS-CV"];
            }
            else if (
              context.Response != null &&
              !string.IsNullOrWhiteSpace(context.Response.Headers["MS-CV"]))
            {
                requestTelemetry.Context.Properties["MS-CV"] = context.Response.Headers["MS-CV"];
            }
        }
    }
}
