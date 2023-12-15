﻿using Markdown.Token;
using Markdown.Tag;

namespace Markdown.Syntax;

public interface ISyntax
{
    ITag ConvertTag(TagType type);
    TagType GetTagType(string tag);
}