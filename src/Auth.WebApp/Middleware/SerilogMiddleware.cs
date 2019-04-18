using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Events;

namespace Auth.WebApp.Middleware
{
    class SerilogMiddleware
    {
        private const string MessageTemplate =
            "{Protocol} {RequestMethod} {RequestPath} {StatusCode} {Type} in {Elapsed:0.0000}ms";

        private const string ErrorTemplate =
            "{Protocol} {RequestMethod} {RequestPath} {StatusCode} {Type} in {Elapsed:0.0000}ms {NewLine}" +
            "host: {Host} headers: {@Headers}";

        private static readonly ILogger Log = Serilog.Log.ForContext<SerilogMiddleware>();

        private static readonly HashSet<string> HeaderWhitelist = new HashSet<string>
            {"Content-Type", "Content-Length", "User-Agent"};

        private readonly RequestDelegate _next;

        public SerilogMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            var start = Stopwatch.GetTimestamp();
            try
            {
                await _next(httpContext);
                var stop = Stopwatch.GetTimestamp();
                string path = GetPath(httpContext);
                var elapsedMs = GetElapsedMilliseconds(start, stop);
                var statusCode = httpContext.Response?.StatusCode;
                var type = httpContext.Response?.ContentType;
                var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;
                var request = httpContext.Request;
                Log.Write(level, MessageTemplate, request.Protocol, request.Method, path, statusCode, type,
                    elapsedMs);
            }
            // Never caught, because `LogException()` returns false.
            catch (Exception ex) when (LogException(httpContext,
                GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()), ex))
            {
            }
        }

        private static bool LogException(HttpContext httpContext, double elapsedMs, Exception ex)
        {
            var path = GetPath(httpContext);
            var host = httpContext.Request.Host.ToString();
            var headers = httpContext.Request.Headers
                .Where(h => HeaderWhitelist.Contains(h.Key))
                .ToDictionary(h => h.Key, h => h.Value.ToString());

            Log.Error(ex, ErrorTemplate,
                httpContext.Request.Protocol,
                httpContext.Request.Method,
                path,
                500,
                elapsedMs,
                host,
                headers);
            // dont catch exception just log it
            return false;
        }

        private static double GetElapsedMilliseconds(long start, long stop)
            => (stop - start) * 1000 / (double) Stopwatch.Frequency;

        private static string GetPath(HttpContext httpContext) =>
            httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ??
            httpContext.Request.Path.ToString();
    }
}