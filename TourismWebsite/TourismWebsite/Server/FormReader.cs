using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Text;

namespace TourismServer.Server;

public static class FormReader
{
    public static async Task<Dictionary<string, string>> ReadAsync(HttpContext ctx)
    {
        using var reader = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();

        var dict = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(body))
            return dict;

        var pairs = body.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in pairs)
        {
            var kv = p.Split('=', 2);
            var key = WebUtility.UrlDecode(kv[0]);
            var value = kv.Length > 1 ? WebUtility.UrlDecode(kv[1]) : "";
            dict[key] = value;
        }

        return dict;
    }
}