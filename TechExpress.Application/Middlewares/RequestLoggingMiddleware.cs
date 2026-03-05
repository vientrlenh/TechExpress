using System;

namespace TechExpress.Application.Middlewares;

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
        var ipAddress = context.Connection.RemoteIpAddress;
        var method = context.Request.Method;
        var path = context.Request.Path;
        var query = context.Request.QueryString;

        _logger.LogInformation("Incoming request: {Method} {Path} {Query} from {IPAddress}", method, path, query, ipAddress);

        await _next(context);
    }
}
