using System.Text.RegularExpressions;
using TourismServer.Server;

namespace TourismServer.Routing;

public sealed class Route
{
    public string Method { get; }
    public string Template { get; }
    public Func<HttpContext, Task> Handler { get; }

    private readonly Regex _regex;
    private readonly Dictionary<string, string> _kinds;

    public Route(string method, string template, Func<HttpContext, Task> handler)
    {
        Method = method.ToUpperInvariant();
        Template = Normalize(template);
        Handler = handler;

        (_regex, _kinds) = CompileTemplate(Template);
    }

    public bool Matches(string method, string path, out Dictionary<string, string> parameters)
    {
        parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!string.Equals(Method, method, StringComparison.OrdinalIgnoreCase))
            return false;

        path = Normalize(path);

        var m = _regex.Match(path);
        if (!m.Success) return false;

        foreach (var kv in _kinds)
        {
            var name = kv.Key;
            var kind = kv.Value;
            var value = m.Groups[name].Value;

            if (kind == "int" && !int.TryParse(value, out _))
                return false;

            parameters[name] = value;
        }

        return true;
    }

    private static string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "/";
        s = s.Trim();
        if (!s.StartsWith('/')) s = "/" + s;
        if (s.Length > 1 && s.EndsWith('/')) s = s.TrimEnd('/');
        return s;
    }

    private static (Regex regex, Dictionary<string, string> kinds) CompileTemplate(string template)
    {
        var kinds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var pattern = Regex.Replace(template, @"\{([a-zA-Z_][a-zA-Z0-9_]*)(:int)?\}", m =>
        {
            var name = m.Groups[1].Value;
            var isInt = m.Groups[2].Success;

            kinds[name] = isInt ? "int" : "string";
            return isInt ? $"(?<{name}>\\d+)" : $"(?<{name}>[^/]+)";
        });

        pattern = "^" + pattern + "$";
        return (new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase), kinds);
    }
}