using System.Text;
using TourismServer.Server;

namespace TourismServer.Results;

public sealed class HtmlResult : IActionResult
{
    private readonly string _html;
    private readonly int _statusCode;

    public HtmlResult(string html, int statusCode = 200)
    {
        _html = html;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpContext ctx)
    {
        ctx.Response.StatusCode = _statusCode;
        ctx.Response.ContentType = "text/html; charset=utf-8";

        var bytes = Encoding.UTF8.GetBytes(_html);
        await ctx.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
    }
}