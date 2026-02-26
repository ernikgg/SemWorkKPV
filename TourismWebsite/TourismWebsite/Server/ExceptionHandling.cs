using TourismServer.Server;

namespace TourismServer.Server;

public sealed class ExceptionHandlingMiddleware
{
    private readonly Func<HttpContext, Task> _next;

    public ExceptionHandlingMiddleware(Func<HttpContext, Task> next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[500] {ctx.Method} {ctx.Path}\n{ex}");

            if (ctx.Response.OutputStream.CanWrite)
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.ContentType = "text/html; charset=utf-8";

                var html = """
                <html>
                  <head><title>500</title></head>
                  <body style="font-family:Arial">
                    <h1>500 — Server error</h1>
                    <p>Something went wrong.</p>
                  </body>
                </html>
                """;

                var bytes = System.Text.Encoding.UTF8.GetBytes(html);
                await ctx.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }
}