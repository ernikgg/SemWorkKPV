using TourismServer.Data;
using TourismServer.Results;
using TourismServer.Server;
using TourismServer.Auth;
using TourismServer.Controllers;
using TourismServer.ViewEngine;

public sealed class ToursController : ControllerBase
{
    private readonly ITourRepository _repo;

    public ToursController(ViewEngine views, ITourRepository repo) : base(views)
    {
        _repo = repo;
    }

    public async Task<IActionResult> DetailsAsync(HttpContext ctx, int id, CancellationToken ct = default)
    {
        var uid = AuthCookie.GetUserId(ctx);

        var tour = await _repo.GetByIdAsync(id, ct);
        if (tour is null)
            return new HtmlResult("<h1>404 Tour Not Found</h1>", statusCode: 404);

        var model = new
        {
            IsAuthenticated = uid is not null,
            ShowGuestLinks = uid is null,

            Tour = tour
        };

        return View("Tours/Details", model);
    }

    public async Task<IActionResult> ListAsync(HttpContext ctx, CancellationToken ct = default)
    {
        var uid = AuthCookie.GetUserId(ctx);

        var tours = await _repo.GetAllAsync(ct);

        var model = new
        {
            IsAuthenticated = uid is not null,
            ShowGuestLinks = uid is null,

            Tours = tours
        };

        return View("Tours/List", model);
    }
}