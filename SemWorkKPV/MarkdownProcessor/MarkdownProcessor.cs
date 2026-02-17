using MarkdownProcessor.Classes;
using MarkdownProcessor.Interfaces;

namespace MarkdownProcessor;

using Markdown.Classes;
using System.Text;

public class MarkdownProcessor
{
    private readonly IParser _parser = new Parser();
    private readonly IRenderer _renderer = new Renderer();

    public async Task<string> ConvertToHtml(string text)
    {
        if (text is null) return "";

        text = text.Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = text.Split('\n');

        var sb = new StringBuilder();

        foreach (var line in lines)
        {
            // если хочешь сохранять пустые строки как переносы:
            if (string.IsNullOrWhiteSpace(line))
            {
                sb.AppendLine("<br/>");
                continue;
            }

            var tokenedLine = await _parser.ParseToTokens(line);
            var htmlLine = await _renderer.Render(tokenedLine.Tokens!, tokenedLine.Line);

            sb.AppendLine(htmlLine);
        }

        return sb.ToString();
    }
}
