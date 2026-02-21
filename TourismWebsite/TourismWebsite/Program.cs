using TourismServer.Controllers;
using TourismServer.Data;
using TourismServer.Models;
using TourismServer.Orm;
using TourismServer.Orm.Core;
using TourismServer.Orm.Validation;
using TourismServer.Routing;
using TourismServer.Server;
using TourismServer.Static;
using TourismServer.ViewEngine;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var router = new Router();

var contentRoot = AppContext.BaseDirectory;

var viewsRoot = Path.Combine(contentRoot, "Views");
var viewEngine = new ViewEngine(viewsRoot);

// TODO: вынесем в конфиг позже
var connString = Environment.GetEnvironmentVariable("DB_CONNECTION")
               ?? "Host=localhost;Port=5432;Database=tourism;Username=postgres;Password=14735K264L";


ITourRepository tourRepo = new PgTourRepository(connString);

var home = new HomeController(viewEngine, tourRepo);
var tours = new ToursController(viewEngine, tourRepo);

router.Get("/", async ctx =>
{
    var result = await home.IndexAsync(cts.Token);
    await result.ExecuteAsync(ctx);
});

var staticFiles = new StaticFileMiddleware(Path.Combine(contentRoot, "wwwroot"));

async Task Pipeline(HttpContext ctx)
{
    if (await staticFiles.TryHandleAsync(ctx))
        return;

    if (ctx.Path.StartsWith("/tours/", StringComparison.OrdinalIgnoreCase))
    {
        var tail = ctx.Path["/tours/".Length..].Trim('/');

        if (int.TryParse(tail, out var id))
        {
            var result = await tours.DetailsAsync(id, cts.Token);
            await result.ExecuteAsync(ctx);
            return;
        }
    }

    if (ctx.Path.Equals("/tours", StringComparison.OrdinalIgnoreCase)
    || ctx.Path.Equals("/tours/", StringComparison.OrdinalIgnoreCase))
    {
        var result = await tours.ListAsync(cts.Token);
        await result.ExecuteAsync(ctx);
        return;
    }
    if (await router.TryHandleAsync(ctx))
        return;


    ctx.Response.StatusCode = 404;
    ctx.Response.ContentType = "text/plain; charset=utf-8";
    var bytes = System.Text.Encoding.UTF8.GetBytes("404 Not Found");
    await ctx.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
}


/*test

await using var ctxOrm = new OrmDbContext(connString);

// 1) получаем список туров из ORM
var tourList = await ctxOrm.Set<Tour>().ToListAsync(cts.Token);

// 2) догружаем Category
await ctxOrm.LoadAsync(tourList, t => t.Category, cts.Token);

Console.WriteLine($"Tours: {tourList.Count}");
foreach (var t in tourList)
{
    Console.WriteLine($"#{t.Id} {t.Title} | cat={(t.Category?.Name ?? "null")}");
}

test */

//test

/* test
Console.WriteLine("[VALIDATION TEST] start");

try
{
    await using var ctxOrm = new OrmDbContext(connString);

    var bad = new Tour
    {
        Title = "",                 // Required нарушен
        PriceText = "x",
        DurationText = "x",
        ImageUrl = "x",
        IsTop = false
    };

    await ctxOrm.Set<Tour>().AddAsync(bad, cts.Token);

    Console.WriteLine("[VALIDATION TEST] FAILED (should have thrown)");
}
catch (ValidationException ex)
{
    Console.WriteLine("[VALIDATION TEST] OK");
    foreach (var e in ex.Errors)
        Console.WriteLine(" - " + e);
}

Console.WriteLine("[VALIDATION TEST] end");

 test validation */
var server = new HttpServer("http://localhost:8080/", Pipeline);
await server.StartAsync(cts.Token);