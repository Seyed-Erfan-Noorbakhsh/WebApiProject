using System.Diagnostics;
using System.Text;

namespace Shop_ProjForWeb.Infrastructure.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestBody = await ReadRequestBodyAsync(context.Request);

        _logger.LogInformation(
            "HTTP {Method} {Path} started. Request Body: {RequestBody}",
            context.Request.Method,
            context.Request.Path,
            requestBody);

        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var responseBodyContent = await ReadResponseBodyAsync(context.Response);
            await responseBody.CopyToAsync(originalBodyStream);

            _logger.LogInformation(
                "HTTP {Method} {Path} completed in {ElapsedMilliseconds}ms with status {StatusCode}. Response Body: {ResponseBody}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode,
                responseBodyContent);
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!request.Body.CanSeek)
        {
            request.EnableBuffering();
        }

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        // Limit body size for logging
        return body.Length > 1000 ? body.Substring(0, 1000) + "..." : body;
    }

    private async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        // Limit body size for logging
        return body.Length > 1000 ? body.Substring(0, 1000) + "..." : body;
    }
}

