using MarkdownProcessor.Enums;
using MarkdownProcessor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownProcessor.Classes;

public class Renderer : IRenderer
{
    private static readonly Dictionary<Style, string[]> StyleToHtml = new(){
        { Style.Italic, ["<em>", "</em>"] }, { Style.Bold, ["<strong>", "</strong>"] },
        { Style.Header1, ["<h1>", "</h1>"] }, { Style.Header2, ["<h2>", "</h2>"] },
        { Style.Header3, ["<h3>", "</h3>"] }, { Style.Header4, ["<h4>", "</h4>"] },
        { Style.Header5, ["<h5>", "</h5>"] }, { Style.Header6, ["<h6>", "</h6>"] },
        { Style.Shielding, ["", ""] }
    };

    private static readonly Dictionary<Style, string> StyleToMd = new()
{
    { Style.Italic, "*" },          // было "_"
    { Style.Bold, "**" },           // было "__"
    { Style.Header1, "#" },
    { Style.Header2, "##" },
    { Style.Header3, "###" },
    { Style.Header4, "####" },
    { Style.Header5, "#####" },
    { Style.Header6, "######" },
    { Style.Shielding, "\\" }
    };

    public async Task<string> Render(List<Token?> tokens, string inputLine)
    {
        return await Task.Run(async () =>
        {
            if (tokens.Count == 0)
                return inputLine;
            var headerToken = tokens.FirstOrDefault(t =>
    t is not null &&
    (t.Style == Style.Header1 || t.Style == Style.Header2 || t.Style == Style.Header3 ||
     t.Style == Style.Header4 || t.Style == Style.Header5 || t.Style == Style.Header6));

            if (headerToken is not null)
            {
                // определяем уровень
                var level = headerToken.Style switch
                {
                    Style.Header1 => 1,
                    Style.Header2 => 2,
                    Style.Header3 => 3,
                    Style.Header4 => 4,
                    Style.Header5 => 5,
                    Style.Header6 => 6,
                    _ => 1
                };

                // убираем ведущие #... и один пробел после них
                // поддержка "# Hello" / "## Hello"
                var i = 0;
                while (i < inputLine.Length && inputLine[i] == '#') i++;
                if (i < inputLine.Length && inputLine[i] == ' ') i++;

                var content = inputLine.Substring(i).TrimEnd();

                return $"<h{level}>{content}</h{level}>";
            }

            var tokensStartIndexSort = tokens.OrderBy(x => x!.StartIndex)
                                                        .Where(x => x!.StartIndex != -1)
                                                        .ToList();
            var tokensEndIndexSort = tokens.OrderBy(x => x!.EndIndex)
                                                        .ToList();

            var lastEndTagIndex = tokensEndIndexSort.Count - 1;
            var lastStartTagIndex = tokensStartIndexSort.Count - 1;

            var listInputTokens = new List<Token?>();

            var stringBuilder = new StringBuilder(inputLine);

            for (var index = inputLine.Length - 1; index >= 0; index--)
            {
                if (lastStartTagIndex > -1 && listInputTokens.Count > 0)
                {
                    var tokenStartIndex = tokensStartIndexSort[lastStartTagIndex];

                    if (tokenStartIndex!.StartIndex == index)
                    {
                        if (!(await ThereAreDigits(inputLine, tokenStartIndex.StartIndex, tokenStartIndex.EndIndex))
                            && listInputTokens.Contains(tokenStartIndex))
                        {
                            stringBuilder.Replace(StyleToMd[tokenStartIndex.Style],
                                StyleToHtml[tokenStartIndex.Style][0], tokenStartIndex.StartIndex,
                                StyleToMd[tokenStartIndex.Style].Length);

                            listInputTokens.Remove(tokenStartIndex);
                        }

                        lastStartTagIndex--;
                    }
                }

                if (lastEndTagIndex <= -1) continue;

                var tokenEndIndex = tokensEndIndexSort[lastEndTagIndex];

                if (tokenEndIndex!.Style == Style.Shielding)
                {
                    stringBuilder.Replace(StyleToMd[tokenEndIndex.Style],
                        StyleToHtml[tokenEndIndex.Style][0], tokenEndIndex.EndIndex,
                        StyleToMd[tokenEndIndex.Style].Length);

                    lastEndTagIndex--;
                }

                else if (tokenEndIndex.StartIndex > -1
                         && (await ThereAreDigits(inputLine, tokenEndIndex.StartIndex, tokenEndIndex.EndIndex)))
                    lastEndTagIndex--;

                else if (listInputTokens.Count > 0
                         && tokenEndIndex.Style != listInputTokens[^1]!.Style
                         && listInputTokens[^1] is { Style: Style.Italic }
                         && tokenEndIndex.StartIndex > listInputTokens[^1]!.StartIndex)
                {
                    lastEndTagIndex--;
                }

                else if (tokenEndIndex.EndIndex == index)
                {
                    stringBuilder.Replace(StyleToMd[tokenEndIndex.Style],
                        StyleToHtml[tokenEndIndex.Style][1], index,
                        StyleToMd[tokenEndIndex.Style].Length);

                    listInputTokens.Add(tokenEndIndex);
                    lastEndTagIndex--;
                }
            }

            return stringBuilder.ToString();
        });
    }

    private async Task<bool> ThereAreDigits(string line, int start, int end)
    {
        return await Task.Run(() =>
        {
            for (var index = start; index < end + 1; index++)
            {
                if (char.IsDigit(line[index]))
                    return true;
            }

            return false;
        });
    }
}