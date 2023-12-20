﻿using System.Diagnostics;
using FluentAssertions;
using Markdown;
using Markdown.Converter;
using Markdown.Processor;
using Markdown.Syntax;

namespace MarkdownTests;

public class MarkupRendererTests
{
    private MarkupRenderer sut;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var syntax = new MarkdownSyntax();
        sut = new MarkupRenderer(syntax, new AnySyntaxParser(syntax), new MarkupConverter(syntax));
    }

    [TestCaseSource(typeof(MarkupRendererTestCases), nameof(MarkupRendererTestCases.RenderTestCases))]
    public void MarkupRenderer_Should(string input, string expectedString)
    {
        var renderedString = sut.Render(input);

        renderedString.Should().Be(expectedString);
    }
    
    [Test]
    public void MarkupRenderer_ShouldHaveLinearComplexity()
    {
        var repetitionsCount = 100;
        var inputString = "#Text___with_ different__ tags\\__";
        
        var shortString = string.Concat(Enumerable.Repeat(inputString, repetitionsCount));
        var longString = string.Concat(Enumerable.Repeat(inputString, repetitionsCount * repetitionsCount));

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        sut.Render(shortString);
        stopwatch.Stop();
        
        var shortStringTime = stopwatch.ElapsedMilliseconds;
        
        stopwatch.Start();
        sut.Render(longString);
        stopwatch.Stop();
        
        var longStringTime = stopwatch.ElapsedMilliseconds;

        longStringTime.Should().BeLessOrEqualTo((long)(shortStringTime * repetitionsCount * 1.1));
    }
}