using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownProcessor.Classes;

public class TokenedLine(string line, List<Token> tokens)
{
    public string Line { get; set; } = line;
    public List<Token> Tokens { get; set; } = tokens;
}
