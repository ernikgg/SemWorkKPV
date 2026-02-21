using TourismServer.Data;
using TourismServer.Results;

namespace TourismServer.Controllers;

public sealed class ToursController : ControllerBase
{
    private readonly ITourRepository _repo;

    public ToursController(ViewEngine.ViewEngine views, ITourRepository repo) : base(views)
    {
        _repo = repo;
    }

    public async Task<IActionResult> DetailsAsync(int id, CancellationToken ct = default)
    {
        var tour = await _repo.GetByIdAsync(id, ct);
        if (tour is null)
            return new HtmlResult("<h1>404 Tour Not Found</h1>", statusCode: 404);

        var model = new { Tour = tour };
        return View("Tours/Details", model);
    }
    public async Task<IActionResult> ListAsync(CancellationToken ct = default)
    {
        var tours = await _repo.GetAllAsync(ct);

        var model = new
        {
            Tours = tours
        };

        return View("Tours/List", model);
    }
}