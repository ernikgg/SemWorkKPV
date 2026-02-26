using System.Net;
using System.Text;
using Npgsql;
using TourismServer.Controllers;
using TourismServer.Results;
using TourismServer.Server;
using TourismServer.ViewEngine;

public sealed class AuthController : ControllerBase
{
    private readonly string _cs;

    public AuthController(ViewEngine views, string connectionString) : base(views)
    {
        _cs = connectionString;
    }


    public async Task<IActionResult> LoginPostAsync(HttpContext ctx, CancellationToken ct = default)
    {
        
        using var sr = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
        var body = await sr.ReadToEndAsync(ct);

        var form = ParseForm(body);
        var email = form.GetValueOrDefault("email")?.Trim() ?? "";
        var password = form.GetValueOrDefault("password") ?? "";

        if (email.Length == 0 || password.Length == 0)
            return Redirect("/?login=1"); 

        var userId = await TryLoginAsync(email, password, ct);
        if (userId is null)
            return Redirect("/?login=1");

        ctx.Response.AppendHeader("Set-Cookie",
            $"auth_uid={userId.Value}; Path=/; HttpOnly; SameSite=Lax");

        return Redirect("/");
    }

    public Task<IActionResult> LogoutAsync(HttpContext ctx, CancellationToken ct = default)
    {
        ctx.Response.AppendHeader("Set-Cookie",
            "auth_uid=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; HttpOnly; SameSite=Lax");

        return Task.FromResult<IActionResult>(Redirect("/"));
    }

    private async Task<int?> TryLoginAsync(string email, string password, CancellationToken ct)
    {
        const string sql = """
            SELECT id
            FROM users
            WHERE email = @email
              AND password_hash = crypt(@password, password_hash)
            LIMIT 1;
            """;

        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("email", email);
        cmd.Parameters.AddWithValue("password", password);

        var obj = await cmd.ExecuteScalarAsync(ct);
        if (obj is null || obj is DBNull) return null;

        return Convert.ToInt32(obj);
    }

    private static Dictionary<string, string> ParseForm(string body)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            var key = WebUtility.UrlDecode(kv[0]);
            var val = kv.Length > 1 ? WebUtility.UrlDecode(kv[1]) : "";
            dict[key] = val;
        }
        return dict;
    }
    public async Task<IActionResult> SignUpPostAsync(HttpContext ctx, CancellationToken ct = default)
    {
        using var sr = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
        var body = await sr.ReadToEndAsync(ct);

        var form = ParseForm(body);
        var email = (form.GetValueOrDefault("email") ?? "").Trim();
        var pass1 = form.GetValueOrDefault("password") ?? "";
        var pass2 = form.GetValueOrDefault("password2") ?? "";

        if (email.Length == 0 || pass1.Length < 6 || pass1 != pass2)
        return new RedirectResult("/?signup=1");

        var createdId = await TryCreateUserAsync(email, pass1, ct);
        if (createdId is null)
            return new RedirectResult("/?signup=1"); 

        ctx.Response.AppendHeader("Set-Cookie",
            $"auth_uid={createdId.Value}; Path=/; HttpOnly; SameSite=Lax");

        return new RedirectResult("/");
    }

    private async Task<int?> TryCreateUserAsync(string email, string password, CancellationToken ct)
    {
        const string sql = """
        INSERT INTO users(email, password_hash)
        VALUES (@email, crypt(@password, gen_salt('bf')))
        ON CONFLICT (email) DO NOTHING
        RETURNING id;
        """;

        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("email", email);
        cmd.Parameters.AddWithValue("password", password);

        var obj = await cmd.ExecuteScalarAsync(ct);
        if (obj is null || obj is DBNull) return null;

        return Convert.ToInt32(obj);
    }
    public Task<IActionResult> LogoutPostAsync(HttpContext ctx, CancellationToken ct = default)
    {
        ctx.Response.AppendHeader("Set-Cookie",
            "auth_uid=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; HttpOnly; SameSite=Lax");

        return Task.FromResult<IActionResult>(new RedirectResult("/"));
    }
}
