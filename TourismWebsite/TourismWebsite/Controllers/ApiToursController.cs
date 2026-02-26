using TourismServer.Data;
using TourismServer.Results;
using TourismServer.Server;

namespace TourismServer.Controllers;

public sealed class ApiToursController
{
    private readonly ITourRepository _repo;

    public ApiToursController(ITourRepository repo) => _repo = repo;

    public async Task<IActionResult> ListAsync(HttpContext ctx, CancellationToken ct = default)
    {
        var q = QueryString.Get(ctx, "q").Trim();

        var all = await _repo.GetAllAsync(ct);

        var filtered = string.IsNullOrWhiteSpace(q)
            ? all
            : all.Where(t => t.Title.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        return new JsonResult(new
        {
            ok = true,
            items = filtered.Select(t => new
            {
                t.Id,
                t.Title,
                t.PriceText,
                t.DurationText,
                t.ImageUrl,
                t.IsTop
            })
        });
    }
}