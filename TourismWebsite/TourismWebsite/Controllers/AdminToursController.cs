using TourismServer.Auth;
using TourismServer.Data;
using TourismServer.Results;
using TourismServer.Server;
using TourismServer.Models.Admin;

namespace TourismServer.Controllers;

public sealed class AdminToursController : ControllerBase
{
    private readonly ITourRepository _repo;
    private readonly AuthService _auth;

    public AdminToursController(ViewEngine.ViewEngine views, ITourRepository repo, AuthService auth) : base(views)
    {
        _repo = repo;
        _auth = auth;
    }

    public async Task<IActionResult> IndexAsync(HttpContext ctx, CancellationToken ct = default)
    {
        if (!await _auth.IsAdminAsync(ctx, ct))
            return new RedirectResult("/");
        if (AuthCookie.GetUserId(ctx) is null)
            return new RedirectResult("/");

        var tours = await _repo.GetAllAsync(ct);
        return View("Admin/Tours/Index", new { Tours = tours });
    }
    public Task<IActionResult> CreateGetAsync(HttpContext ctx, CancellationToken ct = default)
    {

        if (AuthCookie.GetUserId(ctx) is null)
            return Task.FromResult<IActionResult>(new RedirectResult("/"));

        return Task.FromResult<IActionResult>(View("Admin/Tours/Create", new { }));
    }

    public async Task<IActionResult> CreatePostAsync(HttpContext ctx, CancellationToken ct = default)
    {
        if (!await _auth.IsAdminAsync(ctx, ct))
            return new RedirectResult("/");
        if (AuthCookie.GetUserId(ctx) is null)
            return new RedirectResult("/");

        var form = await FormReader.ReadAsync(ctx);

        var model = new TourEditModel
        {
            Title = form["Title"],
            PriceText = form["PriceText"],
            DurationText = form["DurationText"],
            ImageUrl = form["ImageUrl"],
            IsTop = form["IsTop"] == "on" || form["IsTop"] == "true"
        };

        if (string.IsNullOrWhiteSpace(model.Title))
            return View("Admin/Tours/Create", new { Error = "Title is required" });

        await _repo.CreateAsync(model, ct);
        return new RedirectResult("/admin/tours");
    }
    
    public async Task<IActionResult> EditGetAsync(HttpContext ctx, int id, CancellationToken ct = default)
    {
        if (!await _auth.IsAdminAsync(ctx, ct))
            return new RedirectResult("/");
        if (AuthCookie.GetUserId(ctx) is null)
            return new RedirectResult("/");

        var tour = await _repo.GetEditModelByIdAsync(id, ct);
        if (tour is null)
            return new RedirectResult("/admin/tours");

        return View("Admin/Tours/Edit", new { Id = id, Tour = tour });
    }

    public async Task<IActionResult> EditPostAsync(HttpContext ctx, int id, CancellationToken ct = default)
    {
        if (!await _auth.IsAdminAsync(ctx, ct))
            return new RedirectResult("/");
        if (AuthCookie.GetUserId(ctx) is null)
            return new RedirectResult("/");


        var form = await FormReader.ReadAsync(ctx);

        var model = new TourEditModel
        {
            Title = form.GetValueOrDefault("Title", ""),
            PriceText = form.GetValueOrDefault("PriceText", ""),
            DurationText = form.GetValueOrDefault("DurationText", ""),
            ImageUrl = form.GetValueOrDefault("ImageUrl", ""),
            IsTop = form.GetValueOrDefault("IsTop") is "on" or "true"
        };

        if (string.IsNullOrWhiteSpace(model.Title))
            return View("Admin/Tours/Edit", new { Id = id, Tour = model, Error = "Title is required" });

        await _repo.UpdateAsync(id, model, ct);
        return new RedirectResult("/admin/tours");
    }
    public async Task<IActionResult> DeletePostAsync(HttpContext ctx, int id, CancellationToken ct = default)
    {
        if (!await _auth.IsAdminAsync(ctx, ct))
            return new RedirectResult("/");
        if (AuthCookie.GetUserId(ctx) is null)
            return new RedirectResult("/");

        await _repo.DeleteAsync(id, ct);
        return new RedirectResult("/admin/tours");
    }
    public async Task<IActionResult> CreateAjaxAsync(HttpContext ctx, CancellationToken ct = default)
    {
        if (AuthCookie.GetUserId(ctx) is null)
            return new JsonResult(new { ok = false, error = "Unauthorized" }, 401);

        var form = await FormReader.ReadAsync(ctx);

        var model = new TourEditModel
        {
            Title = form.GetValueOrDefault("Title", ""),
            PriceText = form.GetValueOrDefault("PriceText", ""),
            DurationText = form.GetValueOrDefault("DurationText", ""),
            ImageUrl = form.GetValueOrDefault("ImageUrl", ""),
            IsTop = (form.GetValueOrDefault("IsTop", "") == "on") || (form.GetValueOrDefault("IsTop", "") == "true")
        };


        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(model.Title)) errors.Add("Title is required");
        if (string.IsNullOrWhiteSpace(model.PriceText)) errors.Add("PriceText is required");
        if (string.IsNullOrWhiteSpace(model.DurationText)) errors.Add("DurationText is required");
        if (string.IsNullOrWhiteSpace(model.ImageUrl)) errors.Add("ImageUrl is required");

        if (errors.Count > 0)
            return new JsonResult(new { ok = false, errors }, 400);


        var created = await _repo.CreateAsync(model, ct);


        return new JsonResult(new
        {
            ok = true,
            item = created
        });
    }
}