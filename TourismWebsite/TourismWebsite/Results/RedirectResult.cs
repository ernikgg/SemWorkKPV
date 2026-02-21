using TourismServer.Server;

namespace TourismServer.Results;

public sealed class RedirectResult : IActionResult
{
    private readonly string _location;
    private readonly int _statusCode;

    public RedirectResult(string location, int statusCode = 302)
    {
        _location = location;
        _statusCode = statusCode;
    }

    public Task ExecuteAsync(HttpContext ctx)
    {
        ctx.Response.StatusCode = _statusCode;
        ctx.Response.RedirectLocation = _location;
        return Task.CompletedTask;
    }
}