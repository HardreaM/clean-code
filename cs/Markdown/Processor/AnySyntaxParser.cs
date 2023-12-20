﻿using System.Data;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Markdown.Syntax;
using Markdown.Token;

namespace Markdown.Processor;

public class AnySyntaxParser : IParser
{
    private string source;
    private readonly ISyntax syntax;
    private readonly IReadOnlyDictionary<string, Func<int, IToken>> stringToToken;

    public AnySyntaxParser(ISyntax syntax)
    {
        this.syntax = syntax;
        stringToToken = syntax.StringToToken;
    }

    public IList<IToken> ParseTokens(string source)
    {
        this.source = source;
        var tags = FindAllTags();
        tags = RemoveEscapedTags(tags);
        tags = ValidateTagPositioning(tags);
        return tags;
    }

    private IList<IToken> FindAllTags()
    {
        var tags = new List<IToken>();
        var possibleTag = new StringBuilder();
        for (var i = 0; i < source.Length; i++)
        {
            if (stringToToken.Keys.Any(s => s.StartsWith(possibleTag.ToString() + source[i])))
            {
                possibleTag.Append(source[i]);
                continue;
            }

            if (source[i].ToString() == syntax.NewLineSeparator)
            {
                possibleTag.Clear();
                tags.Add(syntax.StringToToken[syntax.NewLineSeparator.ToString()].Invoke(i));
            }

            var tag = possibleTag.ToString();

            if (stringToToken.ContainsKey(tag))
                tags.Add(stringToToken[tag].Invoke(i - tag.Length));

            possibleTag.Clear();

            if (stringToToken.Keys.Any(s => s.StartsWith(possibleTag.ToString() + source[i])))
                possibleTag.Append(source[i]);
        }

        if (possibleTag.Length > 0)
            tags.Add(stringToToken[possibleTag.ToString()].Invoke(source.Length - possibleTag.Length));

        return tags;
    }

    private IList<IToken> RemoveEscapedTags(IList<IToken> tags)
    {
        var result = new List<IToken>();
        var isEscaped = false;
        var escapeIndex = -1;
        IToken escapeToken = null;
        foreach (var tag in tags)
        {
            if (isEscaped)
            {
                isEscaped = false;
                if (tag.Position == escapeIndex + 1)
                {
                    result.Add(escapeToken);
                    continue;
                }
            }

            if (tag.GetType() == syntax.EscapeToken)
            {
                isEscaped = true;
                escapeIndex = tag.Position;
                escapeToken = tag;
            }
            else
                result.Add(tag);
        }

        return result;
    }

    private IList<IToken> ValidateTagPositioning(IList<IToken> tags)
    {
        var result = new List<IToken>();
        var openedTags = new Dictionary<string, IToken>();

        foreach (var tag in tags)
        {
            if (tag.Separator == syntax.NewLineSeparator)
            {
                openedTags.Clear();
                continue;
            }

            if (openedTags.ContainsKey(tag.Separator))
            {
                tag.IsClosed = true;
                if (tag.IsValid(source, ref result) && tag.IsPairedTokenValidPositioned(openedTags[tag.Separator], source))
                {
                    if (openedTags.Values.Any(token =>
                            (!token.IsClosed && token.Position > openedTags[tag.Separator].Position)))
                    {
                        openedTags.Clear();
                        continue;
                    }
                    result.Add(openedTags[tag.Separator]);
                    result.Add(tag);
                    openedTags.Remove(tag.Separator);
                }
            }
            else if (tag.IsValid(source, ref result) && !(syntax.UnsupportedTags.ContainsKey(tag.Separator) &&
                                              syntax.UnsupportedTags[tag.Separator]
                                                  .Any(t => openedTags.ContainsKey(t))))
            {
                if (tag.IsPair)
                    openedTags[tag.Separator] = tag;
                else
                    result.Add(tag);
            }
        }

        return result.Select(token => token).OrderBy(token => token.Position).ToList();
    }
}