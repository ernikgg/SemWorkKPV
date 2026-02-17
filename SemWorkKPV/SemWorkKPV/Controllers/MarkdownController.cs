using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SemWorkKPV.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SemWorkKPV.Options;



[ApiController]
[Route("api/markdown")]
public class MarkdownController : ControllerBase
{
    private readonly MarkdownProcessor.MarkdownProcessor _processor;
    private readonly MarkdownOptions _options;


    public MarkdownController(MarkdownProcessor.MarkdownProcessor processor, IOptions<MarkdownOptions> options)
    {
        _processor = processor;
        _options = options.Value;
    }

    public record RenderRequest(string Markdown);

    [HttpPost("render")]
    public async Task<ActionResult<string>> Render([FromBody] RenderRequest request)
    {
        var md = request?.Markdown ?? "";
        if (md.Length > _options.MaxLength)
            return Problem(
    title: "Markdown too long",
    detail: $"Max length is {_options.MaxLength} characters.",
    statusCode: StatusCodes.Status400BadRequest
);
        var html = await _processor.ConvertToHtml(md);
        return Ok(html);
    }
    [HttpPost("render-html")]
    public async Task<IActionResult> RenderHtml([FromBody] RenderRequest request)
    {
        var md = request?.Markdown ?? "";
        var html = await _processor.ConvertToHtml(md);

        return Content(html, "text/html; charset=utf-8");
    }
}