﻿namespace Markdown.Token;

public class EscapeToken : IToken
{
    private const string TokenSeparator = "\\";
    private const bool HasPair = false;

    public int Length => TokenSeparator.Length;
    public int Position { get; }
    public string Separator => TokenSeparator;
    public bool IsPair => HasPair;
    public bool IsClosed { get; set; }
    
    public EscapeToken(int position)
    {
        Position = position;
    }

    public bool IsValid(string source)
    {
        return true;
    }
}