using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Events;

namespace Template.WebApp
{
    class SerilogMiddleware
    {
        private const string MessageTemplate =
            "{Protocol} {RequestMethod} {RequestPath} {StatusCode} {Type} in {Elapsed:0.0000}ms";

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
                var elapsedMs = GetElapsedMilliseconds(start, stop);
                var statusCode = httpContext.Response?.StatusCode;
                var type = httpContext.Response?.ContentType;
                var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;
                var log = level == LogEventLevel.Error ? LogForErrorContext(httpContext) : Log;
                var request = httpContext.Request;
                log.Write(level, MessageTemplate, request.Protocol, request.Method, GetPath(httpContext), statusCode,type, 
                    elapsedMs);
            }
            // Never caught, because `LogException()` returns false.
            catch (Exception ex) when (LogException(httpContext,
                GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()), ex))
            {
            }
        }

        static bool LogException(HttpContext httpContext, double elapsedMs, Exception ex)
        {
            LogForErrorContext(httpContext)
                .Error(ex, MessageTemplate, httpContext.Request.Protocol, httpContext.Request.Method,
                    GetPath(httpContext), 500, elapsedMs);
            // dont catch exception just log it
            return false;
        }

        static ILogger LogForErrorContext(HttpContext httpContext)
        {
            var request = httpContext.Request;

            var loggedHeaders = request.Headers
                .Where(h => HeaderWhitelist.Contains(h.Key))
                .ToDictionary(h => h.Key, h => h.Value.ToString());
            return Log
                .ForContext("RequestHeaders", loggedHeaders, destructureObjects: true)
                .ForContext("RequestHost", request.Host);
        }

        static double GetElapsedMilliseconds(long start, long stop)
            => (stop - start) * 1000 / (double) Stopwatch.Frequency;

        static string GetPath(HttpContext httpContext) => 
            httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? 
            httpContext.Request.Path.ToString();
    }
}