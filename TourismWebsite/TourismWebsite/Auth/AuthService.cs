using TourismServer.Data;
using TourismServer.Server;

namespace TourismServer.Auth;

public sealed class AuthService
{
    private readonly PgAuthRepository _repo;

    public AuthService(PgAuthRepository repo) => _repo = repo;

    public async Task<bool> IsAdminAsync(HttpContext ctx, CancellationToken ct = default)
    {
        var uid = AuthCookie.GetUserId(ctx);
        if (uid is null) return false;

        var role = await _repo.GetUserRoleAsync(uid.Value, ct);
        return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);
    }
}