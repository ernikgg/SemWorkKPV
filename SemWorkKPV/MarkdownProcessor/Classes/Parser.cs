using MarkdownProcessor.Enums;
using MarkdownProcessor.Interfaces;
using MarkdownProcessor.Classes;
using MarkdownProcessor.Enums;
using MarkdownProcessor.Interfaces;
using System.Text;
using static System.String;

namespace Markdown.Classes;

public class Parser : IParser
{
    public async Task<TokenedLine> ParseToTokens(string line)
    {
        return await Task.Run(async () =>
        {
            var tokensBeParsed = new List<Token?>();

            line = await CheckHeader(tokensBeParsed, line);
            line = await CheckUrl(line);

            var words = line.Split(' ');
            var startSymbolStack = new Stack<Token?>();
            var endSymbolStack = new Stack<Token?>();
            var lengthVerifiedWords = 0;

            foreach (var word in words)
            {
                var tmpStack = new Stack<Token?>();

                for (var index = 0; index < word.Length; index++)
                {
                    if (word[index] == '\\')
                        tokensBeParsed.Add(new Token(-1, index + lengthVerifiedWords, Style.Shielding));

                    if (word[index] != '_'
                        || (index > 0 && word[index - 1] == '\\')
                        || (index > 1 && word[index - 2] == '\\'))
                        continue;

                    var style = index != word.Length - 1 && word[index + 1] == '_' ? Style.Bold : Style.Italic;

                    if (index == 0)
                    {
                        var tmpToken = new Token(lengthVerifiedWords, -1, style);

                        tokensBeParsed.Add(tmpToken);
                        startSymbolStack.Push(tmpToken);
                        tmpStack.Push(tmpToken);
                    }
                    else if (index == word.Length - (style == Style.Italic ? 1 : 2))
                    {

                        if (tmpStack.TryPeek(out var resultTmpStack) && await TryFindTokenEnd(tmpStack, tokensBeParsed,
                                style, index, lengthVerifiedWords))
                        {
                            if (startSymbolStack.TryPeek(out var resultStack) && resultStack != null
                                                                              && resultStack.Equals(resultTmpStack))
                                startSymbolStack.Pop();
                        }
                        else if (!(await TryFindTokenEnd(startSymbolStack, endSymbolStack, tokensBeParsed, style,
                                     index, lengthVerifiedWords)))
                        {
                            var tmpToken = new Token(index + lengthVerifiedWords, -1, style);

                            tokensBeParsed.Add(tmpToken);
                            endSymbolStack.Push(tmpToken);
                        }
                    }
                    else
                    {
                        if (!(await TryFindTokenEnd(tmpStack, tokensBeParsed, style, index, lengthVerifiedWords)))
                        {
                            var tmpToken = new Token(index + lengthVerifiedWords, -1, style);

                            tokensBeParsed.Add(tmpToken);
                            tmpStack.Push(tmpToken);
                        }
                    }

                    index += style == Style.Italic ? 0 : 1;
                }

                lengthVerifiedWords += word.Length + 1;
            }

            return new TokenedLine(line, tokensBeParsed.ToList()!);

        });
    }

    private async Task<string> CheckHeader(List<Token?> tokensBeParsed, string line)
    {
        var stringBuilder = new StringBuilder(line);

        if (stringBuilder[0] == '#')
        {
            var firstTextIndex = 1;

            // Считаем количество решёток, чтобы понять какой уровень у заголовка
            while (stringBuilder[firstTextIndex] == '#' && firstTextIndex < stringBuilder.Length)
                firstTextIndex++;

            // В данном случае firstTextIndex подразумевается как количество решёток в начале
            if (firstTextIndex <= 6)
            {
                if (firstTextIndex == stringBuilder.Length)
                    tokensBeParsed.Add(new Token(0, 0, Style.LineBreak));
                else if (stringBuilder[firstTextIndex] == ' ')
                {
                    stringBuilder.Remove(0, firstTextIndex + 1);

                    if (stringBuilder[^1] == '#')
                    {
                        int lastTextIndex = stringBuilder.Length - 2;

                        while (stringBuilder[lastTextIndex] == '#')
                            lastTextIndex++;

                        if (stringBuilder[lastTextIndex] == ' ')
                        {
                            var isAllSpace = true;
                            for (var i = firstTextIndex; i <= lastTextIndex; i++)
                                isAllSpace &= stringBuilder[i] == ' ';

                            stringBuilder.Remove(lastTextIndex, stringBuilder.Length - lastTextIndex);

                            if (isAllSpace)
                                stringBuilder.Append("<br>");
                            else
                            {
                                var tmpLine = stringBuilder.ToString();

                                stringBuilder.Clear();
                                stringBuilder.Append($"<h{firstTextIndex}>");
                                stringBuilder.Append(tmpLine);
                                stringBuilder.Append($"</h{firstTextIndex}>");
                            }
                        }
                    }
                    else
                    {
                        var tmpLine = stringBuilder.ToString();

                        stringBuilder.Clear();
                        stringBuilder.Append($"<h{firstTextIndex}>");
                        stringBuilder.Append(tmpLine);
                        stringBuilder.Append($"</h{firstTextIndex}>");
                    }
                }
            }
        }

        return stringBuilder.ToString();
    }

    private async Task<string> CheckUrl(string line)
    {
        return await Task.Run(() =>
        {
            var stringBuilder = new StringBuilder();

            for (var index = 0; index < line.Length; index++)
            {
                if (line[index] == '[')
                {
                    var indexEndSquareBrackets = index + 1;
                    while (indexEndSquareBrackets < line.Length
                           && line[indexEndSquareBrackets] != ']')
                        indexEndSquareBrackets++;

                    if (indexEndSquareBrackets != line.Length
                        && line[indexEndSquareBrackets + 1] == '(')
                    {
                        var indexEndBrackets = indexEndSquareBrackets + 2;
                        while (indexEndBrackets < line.Length
                               && line[indexEndBrackets] != ')')
                            indexEndBrackets++;

                        if (indexEndBrackets < line.Length)
                        {
                            var startLine = line[..index];
                            var urlName = line.Substring(index + 1, indexEndSquareBrackets - (index + 1));
                            var url = line
                                .Substring(indexEndSquareBrackets + 2, indexEndBrackets - (indexEndSquareBrackets + 2));
                            var afterUrlLine = line.Substring(indexEndBrackets,
                                line.Length - indexEndBrackets - 1);

                            if (IsAllSpaces(url))
                            {
                                url = "#";
                            }

                            stringBuilder.Append(startLine);
                            stringBuilder.Append($"""<a href="{url}">{urlName}</a>""");
                            stringBuilder.Append(afterUrlLine);
                        }
                    }
                }
            }

            return stringBuilder.ToString() == Empty ? line : stringBuilder.ToString();
        });
    }



    private static bool IsAllSpaces(string line)
    {
        return line.All(t => t == ' ');
    }

    private async Task<bool> TryFindTokenEnd(Stack<Token?> stackTokens, List<Token?> tokens, Style style,
        int index, int lengthVerifiedWords)
    {
        return await Task.Run(() =>
        {
            switch (stackTokens.Count)
            {
                case 0:
                    return false;
                case 3 when stackTokens.Peek()!.Style == style:
                    {
                        var tmpToken = stackTokens.Pop();

                        tmpToken!.EndIndex = index + lengthVerifiedWords;
                        return true;
                    }
                case 3:
                    {
                        while (stackTokens.Count > 0)
                            tokens.Remove(stackTokens.Pop());
                        break;
                    }
                default:
                    {
                        if (stackTokens.Peek()!.Style == style &&
                            stackTokens.Peek()!.StartIndex < (index + lengthVerifiedWords - 2))
                        {
                            var tmpToken = stackTokens.Pop();

                            tmpToken!.EndIndex = index + lengthVerifiedWords;
                            return true;
                        }

                        break;
                    }
            }

            return false;
        });
    }

    private async Task<bool> TryFindTokenEnd(Stack<Token?> startStackTokens, Stack<Token?> endStackTokens,
        List<Token?> tokens, Style style, int index, int lengthVerifiedWords)
    {
        return await Task.Run(() =>
        {
            switch (startStackTokens.Count)
            {
                case 0:
                    return false;
                case 2 when endStackTokens.Count == 1 && startStackTokens.Peek()!.Style == style:
                    {
                        var tmpToken = new Token(index + lengthVerifiedWords, -1, style);

                        startStackTokens.Clear();
                        endStackTokens.Clear();

                        tokens.Add(tmpToken);
                        return true;
                    }
                default:
                    {
                        if (startStackTokens.Peek()!.Style == style &&
                            startStackTokens.Peek()!.StartIndex < index + lengthVerifiedWords - 2)
                        {
                            var tmpToken = startStackTokens.Pop();

                            tmpToken!.EndIndex = index + lengthVerifiedWords;
                            return true;
                        }

                        break;
                    }
            }

            return false;
        });
    }
}