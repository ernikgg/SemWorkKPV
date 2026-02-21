using TourismServer.Server;

namespace TourismServer.Static;

public sealed class StaticFileMiddleware
{
    private readonly string _wwwrootPath;

    public StaticFileMiddleware(string wwwrootPath) => _wwwrootPath = wwwrootPath;

    public async Task<bool> TryHandleAsync(HttpContext ctx)
    {
        if (!ctx.Path.StartsWith("/css/") &&
    !ctx.Path.StartsWith("/js/") &&
    !ctx.Path.StartsWith("/images/") &&
    !ctx.Path.StartsWith("/jadoo/"))
            return false;

        var rel = ctx.Path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var full = Path.Combine(_wwwrootPath, rel);

        if (!File.Exists(full)) return false;

        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = GuessContentType(full);

        await using var fs = File.OpenRead(full);
        await fs.CopyToAsync(ctx.Response.OutputStream);
        return true;
    }

    private static string GuessContentType(string fullPath)
    {
        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        return ext switch
        {
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".ico" => "image/x-icon",
            ".woff2" => "font/woff2",
            ".woff" => "font/woff",
            ".ttf" => "font/ttf",
            _ => "application/octet-stream"

        };
    }
}