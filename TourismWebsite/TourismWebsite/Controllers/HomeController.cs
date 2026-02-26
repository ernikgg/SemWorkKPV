using TourismServer.Auth;
using TourismServer.Data;
using TourismServer.Models;
using TourismServer.Results;
using TourismServer.Server;

namespace TourismServer.Controllers;

public sealed class HomeController : ControllerBase
{
    private readonly ITourRepository _tours;

    public HomeController(ViewEngine.ViewEngine views, ITourRepository tours) : base(views)
    {
        _tours = tours;
    }



    public async Task<IActionResult> IndexAsync(HttpContext ctx, CancellationToken ct = default)
    {
        var uid = AuthCookie.GetUserId(ctx);

        var model = new
        {
            IsAuthenticated = uid is not null,
            AuthUserId = uid, 
            ShowGuestLinks = uid is null,

            HeroTagline = "Best Destinations around the world",
            HeroTitle = "Travel, enjoy and live a new and full life",
            HeroDescription = "Built Wicket longer admire do barton vanity itself do in it. Preferred to sportsmen it engrossed listening. Park gate sell they west hard for the.",
            HeroCtaText = "Find out more",
            HeroDemoText = "Play Demo",

            TopDestinations = await _tours.GetTopAsync(3, ct),
            BookingTour = await _tours.GetByTitleAsync("Trip To Greece", ct)
        };

        return View("Home/Index", model);
    }
}