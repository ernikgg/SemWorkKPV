using System.Net.WebSockets;
using TourismServer.Auth;
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


var connString = Environment.GetEnvironmentVariable("DB_CONNECTION")
               ?? "Host=localhost;Port=5432;Database=tourism;Username=postgres;Password=new_password";


ITourRepository tourRepo = new PgTourRepository(connString);
var authRepo = new PgAuthRepository(connString);
var authService = new AuthService(authRepo);
var home = new HomeController(viewEngine, tourRepo);
var tours = new ToursController(viewEngine, tourRepo);
var auth = new AuthController(viewEngine, connString);
var admin = new AdminController(viewEngine, authService);
var adminTours = new AdminToursController(viewEngine, tourRepo, authService);
var apiTours = new ApiToursController(tourRepo);

router.Get("/api/tours", async ctx =>
{
    var result = await apiTours.ListAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});
router.Post("/auth/login", async ctx =>
{
    var result = await auth.LoginPostAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});
router.Post("/auth/logout", async ctx =>
{
    var result = await auth.LogoutPostAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});

router.Post("/auth/signup", async ctx =>
{
    var result = await auth.SignUpPostAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});
router.Get("/tours", async ctx =>
{
    var result = await tours.ListAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});

router.Get("/tours/{id:int}", async ctx =>
{
    var id = int.Parse(ctx.RouteValues["id"]);
    var result = await tours.DetailsAsync(ctx, id, cts.Token);
    await result.ExecuteAsync(ctx);
});
router.Get("/admin/tours", async ctx =>
{
    var result = await adminTours.IndexAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});

router.Get("/", async ctx =>
{
    var result = await home.IndexAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});
router.Post("/api/admin/tours/create", async ctx =>
{
    var result = await adminTours.CreateAjaxAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});
router.Get("/admin", async ctx =>
{
    var result = await admin.IndexAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});
router.Get("/admin/tours/create", async ctx =>
{
    var result = await adminTours.CreateGetAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});

router.Post("/admin/tours/create", async ctx =>
{
    var result = await adminTours.CreatePostAsync(ctx, cts.Token);
    await result.ExecuteAsync(ctx);
});

var staticFiles = new StaticFileMiddleware(Path.Combine(contentRoot, "wwwroot"));

async Task CorePipeline(HttpContext ctx)
{
    if (await staticFiles.TryHandleAsync(ctx))
        return;

    Console.WriteLine($"REQ {ctx.Method} {ctx.Path}");


    if (ctx.Path.StartsWith("/admin/tours/edit/", StringComparison.OrdinalIgnoreCase))
    {
        var tail = ctx.Path["/admin/tours/edit/".Length..].Trim('/');
        if (int.TryParse(tail, out var id))
        {
            if (ctx.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                var result = await adminTours.EditGetAsync(ctx, id, cts.Token);
                await result.ExecuteAsync(ctx);
                return;
            }

            if (ctx.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                var result = await adminTours.EditPostAsync(ctx, id, cts.Token);
                await result.ExecuteAsync(ctx);
                return;
            }
        }

        ctx.Response.StatusCode = 302;
        ctx.Response.RedirectLocation = "/admin/tours";
        return;
    }

    if (ctx.Path.StartsWith("/admin/tours/delete/", StringComparison.OrdinalIgnoreCase))
    {
        if (!ctx.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Response.StatusCode = 302;
            ctx.Response.RedirectLocation = "/admin/tours";
            return;
        }

        var tail = ctx.Path["/admin/tours/delete/".Length..].Trim('/');
        if (int.TryParse(tail, out var id))
        {
            var result = await adminTours.DeletePostAsync(ctx, id, cts.Token);
            await result.ExecuteAsync(ctx);
            return;
        }

        ctx.Response.StatusCode = 302;
        ctx.Response.RedirectLocation = "/admin/tours";
        return;
    }

    if (await router.TryHandleAsync(ctx))
        return;

    ctx.Response.StatusCode = 302;
    ctx.Response.RedirectLocation = "/";
}

var exceptionMw = new ExceptionHandlingMiddleware(CorePipeline);
async Task Pipeline(HttpContext ctx) => await exceptionMw.InvokeAsync(ctx);

var server = new HttpServer("http://localhost:8080/", Pipeline);
await server.StartAsync(cts.Token);