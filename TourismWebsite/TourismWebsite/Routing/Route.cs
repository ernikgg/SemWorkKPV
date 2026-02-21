namespace TourismServer.Routing;

public sealed class Route
{
    public string Method { get; }
    public string Path { get; }
    public Func<TourismServer.Server.HttpContext, Task> Handler { get; }

    public Route(string method, string path, Func<TourismServer.Server.HttpContext, Task> handler)
    {
        Method = method.ToUpperInvariant();
        Path = NormalizePath(path);
        Handler = handler;
    }

    public bool Matches(string method, string path)
        => Method == method.ToUpperInvariant() && Path == NormalizePath(path);

    private static string NormalizePath(string p)
    {
        if (string.IsNullOrWhiteSpace(p)) return "/";
        if (!p.StartsWith("/")) p = "/" + p;
        if (p.Length > 1 && p.EndsWith("/")) p = p[..^1];
        return p;
    }
}