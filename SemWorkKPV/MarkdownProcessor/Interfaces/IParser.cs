using MarkdownProcessor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownProcessor.Interfaces;

public interface IParser
{
    /// <summary>
    /// Метод для парса строки из MD файла в список токенов
    /// </summary>
    /// <param name="line">Текст файла или путь к нему</param>
    /// <returns>Полученный список токенов</returns>
    Task<TokenedLine> ParseToTokens(string line);
}