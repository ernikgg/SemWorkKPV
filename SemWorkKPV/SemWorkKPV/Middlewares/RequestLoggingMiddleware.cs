using System.Diagnostics;

namespace SemWorkKPV.Middlewares;

public sealed class RequestLoggingMiddleware
{
    private const string TraceHeader = "X-Trace-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        // Берём traceId (если клиент прислал), иначе используем системный
        var traceId = context.Request.Headers.TryGetValue(TraceHeader, out var incoming)
            ? incoming.ToString()
            : context.TraceIdentifier;

        // Положим traceId в response header, чтобы видеть его в браузере/логах
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[TraceHeader] = traceId;
            return Task.CompletedTask;
        });

        // Чтобы во всех логах автоматически был TraceId
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId
        });

        await _next(context);

        sw.Stop();

        _logger.LogInformation(
            "HTTP {Method} {Path} -> {StatusCode} in {ElapsedMs} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
}

