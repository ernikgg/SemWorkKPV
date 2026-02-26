using System.Text;
using System.Text.Json;
using TourismServer.Server;

namespace TourismServer.Results;

public sealed class JsonResult : IActionResult
{
    private readonly object _data;
    private readonly int _statusCode;

    public JsonResult(object data, int statusCode = 200)
    {
        _data = data;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpContext ctx)
    {
        ctx.Response.StatusCode = _statusCode;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        await ctx.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
    }
}