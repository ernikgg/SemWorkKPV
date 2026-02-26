using TourismServer.Auth;
using TourismServer.Results;
using TourismServer.Server;

namespace TourismServer.Controllers;

public sealed class AdminController : ControllerBase
{
    private readonly AuthService _auth;

    public AdminController(ViewEngine.ViewEngine views, AuthService auth)
        : base(views)
    {
        _auth = auth;
    }

    public async Task<IActionResult> IndexAsync(HttpContext ctx, CancellationToken ct = default)
    {
        if (!await _auth.IsAdminAsync(ctx, ct))
            return new RedirectResult("/");

        return View("Admin/Index", new { });
    }
}