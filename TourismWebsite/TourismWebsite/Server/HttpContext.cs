using System.Net;

namespace TourismServer.Server;

public sealed class HttpContext
{
    public HttpListenerContext Raw { get; }

    public HttpListenerRequest Request => Raw.Request;
    public HttpListenerResponse Response => Raw.Response;

    public string Method => Request.HttpMethod.ToUpperInvariant();
    public string Path => Request.Url?.AbsolutePath ?? "/";

    public HttpContext(HttpListenerContext raw) => Raw = raw;
}