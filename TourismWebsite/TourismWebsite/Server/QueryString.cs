using System.Web;

namespace TourismServer.Server;

public static class QueryString
{
    public static string Get(HttpContext ctx, string key)
    {
        var q = ctx.Request.Url?.Query ?? "";
        var nvc = HttpUtility.ParseQueryString(q);
        return nvc[key] ?? "";
    }
}