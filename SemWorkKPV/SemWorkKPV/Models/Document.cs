namespace SemWorkKPV.Models;

public class Document
{
    public int Id { get; set; }

    public string Title { get; set; } = "Untitled";

    public string Markdown { get; set; } = "";

    public string Html { get; set; } = "";
    public int OwnerId { get; set; }


    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
