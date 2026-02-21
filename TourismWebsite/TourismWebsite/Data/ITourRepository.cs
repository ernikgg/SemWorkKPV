using TourismServer.Models;

namespace TourismServer.Data;

public interface ITourRepository
{
    Task<IReadOnlyList<DestinationCard>> GetTopAsync(int count, CancellationToken ct = default);
    Task<DestinationCard?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<DestinationCard>> GetAllAsync(CancellationToken ct = default);
}
