using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TourismServer.ViewEngine;

public sealed class ViewEngine
{
    private static readonly Regex IfBlockRegex =
    new(@"\{%\s*if\s+(?<cond>[\w\.]+)\s*%\}(?<body>.*?)\{%\s*endif\s*%\}",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
    private static readonly Regex ForBlockRegex =
    new(@"\{%\s*for\s+(?<var>\w+)\s+in\s+(?<list>[\w\.]+)\s*%\}(?<body>.*?)\{%\s*endfor\s*%\}",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
    private static readonly Regex TokenRegex = new(@"\{\{\s*(?<expr>[^}]+?)\s*\}\}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly string _viewsRootPath;

    public ViewEngine(string viewsRootPath)
    {
        _viewsRootPath = viewsRootPath;
    }

    public async Task<string> RenderViewAsync(string viewName, object? model = null)
    {
        var relative = viewName.Replace('/', Path.DirectorySeparatorChar) + ".html";
        var fullPath = Path.Combine(_viewsRootPath, relative);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"View not found: {fullPath}");

        var template = await File.ReadAllTextAsync(fullPath, Encoding.UTF8);

        if (model is null) return template;

        // 1) циклы
        template = ForBlockRegex.Replace(template, m =>
        {
            var varName = m.Groups["var"].Value.Trim();      // tour
            var listExpr = m.Groups["list"].Value.Trim();    // TopDestinations
            var body = m.Groups["body"].Value;               // HTML внутри

            var listObj = ResolveExpression(model, listExpr);
            if (listObj is not System.Collections.IEnumerable items)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var item in items)
            {
                // Внутри цикла позволяем обращаться к переменной цикла:
                // {{ tour.Title }}, {{ tour.Price }}
                var localModel = new ScopeModel(model, varName, item);
                var rendered = TokenRegex.Replace(body, mm =>
                {
                    var expr = mm.Groups["expr"].Value.Trim();
                    var v = ResolveExpression(localModel, expr);
                    return v?.ToString() ?? string.Empty;
                });

                sb.Append(rendered);
            }
            return sb.ToString();
        });
        // 2) if
        template = IfBlockRegex.Replace(template, m =>
        {
            var condExpr = m.Groups["cond"].Value.Trim(); // d.IsTop
            var body = m.Groups["body"].Value;

            var condValue = ResolveExpression(model, condExpr);

            if (condValue is bool b && b)
                return body;

            return string.Empty;
        });

        // 2)  {{ }}
        template = TokenRegex.Replace(template, m =>
        {
            var expr = m.Groups["expr"].Value.Trim();
            var value = ResolveExpression(model, expr);
            return value?.ToString() ?? string.Empty;
        });

        return template;
    }

    private static object? ResolveExpression(object model, string expr)
    {
        // Поддержим:
        // 1) HeroTitle
        // 2) Model.HeroTitle (на будущее)
        // 3) вложенные: A.B.C
        var path = expr;

        if (path.StartsWith("Model.", StringComparison.OrdinalIgnoreCase))
            path = path["Model.".Length..];

        object? current = model;
        foreach (var part in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current is null) return null;

            // если это словарь
            if (current is IReadOnlyDictionary<string, object?> rod && rod.TryGetValue(part, out var v1))
            {
                current = v1;
                continue;
            }
            if (current is IDictionary<string, object?> d && d.TryGetValue(part, out var v2))
            {
                current = v2;
                continue;
            }

            var t = current.GetType();
            var prop = t.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop is null) return null;

            current = prop.GetValue(current);
        }

        return current;
    }
    private sealed class ScopeModel : IReadOnlyDictionary<string, object?>
    {
        private readonly object _root;
        private readonly string _varName;
        private readonly object? _varValue;

        public ScopeModel(object root, string varName, object? varValue)
        {
            _root = root;
            _varName = varName;
            _varValue = varValue;
        }

        public object? this[string key] => TryGetValue(key, out var v) ? v : null;

        public IEnumerable<string> Keys => new[] { "Model", _varName };
        public IEnumerable<object?> Values => new object?[] { _root, _varValue };
        public int Count => 2;

        public bool ContainsKey(string key) => key == "Model" || string.Equals(key, _varName, StringComparison.OrdinalIgnoreCase);

        public bool TryGetValue(string key, out object? value)
        {
            if (string.Equals(key, "Model", StringComparison.OrdinalIgnoreCase))
            {
                value = _root;
                return true;
            }
            if (string.Equals(key, _varName, StringComparison.OrdinalIgnoreCase))
            {
                value = _varValue;
                return true;
            }
            value = null;
            return false;
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            yield return new KeyValuePair<string, object?>("Model", _root);
            yield return new KeyValuePair<string, object?>(_varName, _varValue);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
