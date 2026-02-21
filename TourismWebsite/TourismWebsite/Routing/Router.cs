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
            if (r.Matches(ctx.Method, ctx.Path))
            {
                await r.Handler(ctx);
                return true;
            }
        }
        return false;
    }
}