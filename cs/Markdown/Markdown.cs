﻿using Markdown.Converter;
using Markdown.Syntax;
using Markdown.Token;

namespace Markdown;

public class Markdown
{
    public string Render(string text, ISyntax syntax)
    {
        var processor = new Processor.Processor(text, syntax);
        var tagTokens = processor.ParseTags();
        var converter = new HtmlConverter(syntax);
        return converter.ConvertTags(tagTokens, text);
    }
}