using TourismServer.Server;
using TourismServer.ViewEngine;

namespace TourismServer.Results;

public sealed class ViewResult : IActionResult
{
    private readonly ViewEngine.ViewEngine _engine;
    private readonly string _viewName;
    private readonly object? _model;

    public ViewResult(ViewEngine.ViewEngine engine, string viewName, object? model = null)
    {
        _engine = engine;
        _viewName = viewName;
        _model = model;
    }

    public async Task ExecuteAsync(HttpContext ctx)
    {
        var html = await _engine.RenderViewAsync(_viewName, _model);

        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html; charset=utf-8";

        var bytes = System.Text.Encoding.UTF8.GetBytes(html);
        await ctx.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
    }
}
