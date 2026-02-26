using System.Net;
using TourismServer.Server;

namespace TourismServer.Auth;

public static class AuthCookie
{
    public static int? GetUserId(HttpContext ctx)
    {
        var cookie = ctx.Request.Cookies["auth_uid"];
        if (cookie is null) return null;

        return int.TryParse(cookie.Value, out var id) ? id : null;
    }
}