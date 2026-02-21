using System.Net;

namespace TourismServer.Server;

public sealed class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly Func<HttpContext, Task> _pipeline;

    public HttpServer(string prefix, Func<HttpContext, Task> pipeline)
    {
        _listener.Prefixes.Add(prefix);
        _pipeline = pipeline;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _listener.Start();
        Console.WriteLine($"Listening on: {string.Join(", ", _listener.Prefixes)}");

        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext raw;
            try
            {
                raw = await _listener.GetContextAsync();
            }
            catch (HttpListenerException) when (ct.IsCancellationRequested)
            {
                break;
            }

            _ = Task.Run(async () =>
            {
                var ctx = new HttpContext(raw);
                try
                {
                    await _pipeline(ctx);
                }
                catch (Exception ex)
                {
                    await WriteErrorAsync(ctx, ex);
                }
                finally
                {
                    try { ctx.Response.OutputStream.Close(); } catch { /* ignore */ }
                }
            }, ct);
        }

        _listener.Stop();
    }

    private static async Task WriteErrorAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "text/plain; charset=utf-8";
        var msg = $"500 Internal Server Error\n\n{ex}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(msg);
        await ctx.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
    }
}