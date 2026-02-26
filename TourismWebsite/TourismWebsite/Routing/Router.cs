using TourismServer.Server;

namespace TourismServer.Routing;

public sealed class Router
{
    private readonly List<Route> _routes = new();

    public void Get(string path, Func<HttpContext, Task> handler) => _routes.Add(new Route("GET", path, handler));
    public void Post(string path, Func<HttpContext, Task> handler) => _routes.Add(new Route("POST", path, handler));


    public async Task<bool> TryHandleAsync(HttpContext ctx)
    {
        foreach (var r in _routes)
        {
            if (r.Matches(ctx.Method, ctx.Path, out var values))
            {
                Console.WriteLine($"MATCH {ctx.Method} {ctx.Path} -> {r.Template}");

                ctx.RouteValues.Clear();
                foreach (var kv in values)
                    ctx.RouteValues[kv.Key] = kv.Value;

                await r.Handler(ctx);
                return true;
            }
        }

        Console.WriteLine($"NO MATCH {ctx.Method} {ctx.Path}");
        return false;
    }
}