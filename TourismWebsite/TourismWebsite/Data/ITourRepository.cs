using TourismServer.Models;
using TourismServer.Models.Admin;

namespace TourismServer.Data;

public interface ITourRepository
{
    Task<IReadOnlyList<DestinationCard>> GetTopAsync(int count, CancellationToken ct = default);
    Task<DestinationCard?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<DestinationCard>> GetAllAsync(CancellationToken ct = default);
    Task<DestinationCard?> GetByTitleAsync(string title, CancellationToken ct = default);
    Task<int> CreateAsync(TourEditModel model, CancellationToken ct = default);
    Task<TourEditModel?> GetEditModelByIdAsync(int id, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, TourEditModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
