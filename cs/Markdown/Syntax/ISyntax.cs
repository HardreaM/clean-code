﻿using Markdown.Token;
using Markdown.Tag;

namespace Markdown.Syntax;

public interface ISyntax
{
    ITag ConvertTag(IToken token);
    IReadOnlyDictionary<string, Func<int, IToken>> StringToToken { get; }
    Type EscapeToken { get; }
    string NewLineSeparator { get; }
    IReadOnlyDictionary<string, IList<string>> TagCannotBeInsideTags { get; }
    IList<string> GetSupprtedTagParameters(string tagSeparator);
}