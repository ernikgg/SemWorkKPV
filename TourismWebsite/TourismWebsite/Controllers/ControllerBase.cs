using TourismServer.Results;
using TourismServer.ViewEngine;

namespace TourismServer.Controllers;

public abstract class ControllerBase
{
    protected ViewEngine.ViewEngine Views { get; }

    protected ControllerBase(ViewEngine.ViewEngine views)
    {
        Views = views;
    }

    protected IActionResult View(string viewName, object? model = null)
        => new ViewResult(Views, viewName, model);
}