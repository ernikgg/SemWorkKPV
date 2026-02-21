using TourismServer.Server;

namespace TourismServer.Results;

public interface IActionResult
{
    Task ExecuteAsync(HttpContext ctx);
}