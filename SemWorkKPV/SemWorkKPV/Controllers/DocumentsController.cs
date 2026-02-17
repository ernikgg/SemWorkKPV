using MarkdownProcessor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemWorkKPV.Data;
using SemWorkKPV.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;


namespace SemWorkKPV.Controllers;

[Authorize]
[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public DocumentsController(AppDbContext db)
    {
        _db = db;
    }

    public class CreateRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = "Untitled";

        [Required]
        [MaxLength(200_000)] // лимит чтобы не слали мегабайты
        public string Markdown { get; set; } = "";
    }
    private int GetUserId()
    {
        var uid = User.FindFirst("uid")?.Value;
        return int.Parse(uid!);
    }


    public class UpdateRequest
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(200_000)]
        public string? Markdown { get; set; }
    }

    [HttpGet]
    public async Task<ActionResult<List<Document>>> GetAll()
    {
        var userId = GetUserId();
        var docs = await _db.Documents
            .Where(d => d.OwnerId == userId)
            .OrderByDescending(d => d.UpdatedAtUtc)
            .ToListAsync();

        return Ok(docs);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Document>> GetById(int id)
    {
        var userId = GetUserId();
        var doc = await _db.Documents.FindAsync(id);
        if (doc is null) return NotFound();
        if (doc.OwnerId != userId) return Forbid();
        return Ok(doc);
    }

    [HttpPost]
    public async Task<ActionResult<Document>> Create([FromBody] CreateRequest req,
        [FromServices] MarkdownProcessor.MarkdownProcessor processor) // <-- замени MarkdownEngine на свой главный класс, если иначе
    {
        var md = req.Markdown;
        var userId = GetUserId();
        var html = await processor.ConvertToHtml(md); // <-- подстрой под свой метод/await

        var doc = new Document
        {
            Title = string.IsNullOrWhiteSpace(req.Title) ? "Untitled" : req.Title!,
            Markdown = md,
            Html = html,
            OwnerId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = doc.Id }, doc);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Document>> Update(int id, [FromBody] UpdateRequest req,
        [FromServices] MarkdownProcessor.MarkdownProcessor processor) 
    {
        var userId = GetUserId();

        var doc = await _db.Documents.FindAsync(id);
        if (doc is null) return NotFound();
        if (doc.OwnerId != userId)
            return Forbid(); // 403

        if (!string.IsNullOrWhiteSpace(req.Title))
            doc.Title = req.Title!;

        if (req.Markdown is not null)
            doc.Markdown = req.Markdown;

        doc.Html = await processor.ConvertToHtml(doc.Markdown); 
        doc.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(doc);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _db.Documents.FindAsync(id);
        var userId = GetUserId();
        if (doc is null) return NotFound();
        if (doc.OwnerId != userId)
            return Forbid();

        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
