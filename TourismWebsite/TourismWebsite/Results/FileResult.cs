using TourismServer.Server;

namespace TourismServer.Results;

public sealed class FileResult : IActionResult
{
    private readonly string _fullPath;
    private readonly string _contentType;

    public FileResult(string fullPath, string contentType)
    {
        _fullPath = fullPath;
        _contentType = contentType;
    }

    public async Task ExecuteAsync(HttpContext ctx)
    {
        if (!File.Exists(_fullPath))
        {
            ctx.Response.StatusCode = 404;
            ctx.Response.ContentType = "text/plain; charset=utf-8";
            var bytes = System.Text.Encoding.UTF8.GetBytes("404 File not found");
            await ctx.Response.OutputStream.WriteAsync(bytes);
            return;
        }

        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = _contentType;

        await using var fs = File.OpenRead(_fullPath);
        await fs.CopyToAsync(ctx.Response.OutputStream);
    }
}