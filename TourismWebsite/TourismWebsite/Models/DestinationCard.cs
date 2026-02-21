namespace TourismServer.Models;

public sealed class DestinationCard
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string PriceText { get; init; } = "";
    public string DurationText { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public bool IsTop { get; init; }
}