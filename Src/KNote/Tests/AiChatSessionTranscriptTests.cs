using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// AiChatSessionTranscript: the Description of an AI session note, Markdown readable as any note and parsed
/// back into the same turns (the sessions are resumed from it, by ClientWin and by the Web client).
/// </summary>
[TestClass]
public class AiChatSessionTranscriptTests
{
    [TestMethod]
    public void Write_ThenParse_ReturnsTheSameTurns()
    {
        var turns = new List<AiChatTurnDto>
        {
            new()
            {
                Prompt = "First question\r\n\r\nwith a blank line",
                Answer = "```csharp\r\nvar x = 1;\r\n```\r\n\r\n- item",
                InputTokens = 3, OutputTokens = 4, TotalTokens = 7,
                ProcessingTime = TimeSpan.FromMilliseconds(1234)
            },
            new() { Prompt = "Second", Answer = "Cut", TotalTokens = 9, TokensEstimated = true, Truncated = true }
        };

        var parsed = AiChatSessionTranscript.Parse(AiChatSessionTranscript.Write(turns));

        Assert.AreEqual(2, parsed.Count);
        for (var i = 0; i < turns.Count; i++)
        {
            Assert.AreEqual(turns[i].Prompt, parsed[i].Prompt);
            Assert.AreEqual(turns[i].Answer, parsed[i].Answer);
            Assert.AreEqual(turns[i].InputTokens, parsed[i].InputTokens);
            Assert.AreEqual(turns[i].OutputTokens, parsed[i].OutputTokens);
            Assert.AreEqual(turns[i].TotalTokens, parsed[i].TotalTokens);
            Assert.AreEqual(turns[i].TokensEstimated, parsed[i].TokensEstimated);
            Assert.AreEqual(turns[i].Truncated, parsed[i].Truncated);
            Assert.AreEqual(turns[i].ProcessingTime, parsed[i].ProcessingTime);
        }
    }

    [TestMethod]
    public void Write_IsReadableMarkdown_WithHiddenMarkers()
    {
        var text = AiChatSessionTranscript.Write([new AiChatTurnDto { Prompt = "Hi", Answer = "Hello" }]);

        StringAssert.Contains(text, "**User:** \r\nHi\r\n");
        StringAssert.Contains(text, "**Assistant:** \r\nHello\r\n");
        StringAssert.StartsWith(text, "<!-- knt-ai:user -->");
    }

    [TestMethod]
    public void Content_IsStoredWithCrLf_WhateverItCameWith()
    {
        var parsed = AiChatSessionTranscript.Parse(
            AiChatSessionTranscript.Write([new AiChatTurnDto { Prompt = "a\nb", Answer = "c\r\nd\n" }]));

        Assert.AreEqual("a\r\nb", parsed[0].Prompt);
        Assert.AreEqual("c\r\nd", parsed[0].Answer);
    }

    [TestMethod]
    public void Parse_SurvivesHandEdits_TextBeforeTheFirstTurnAndBrokenUsage()
    {
        var text = "My notes about this chat\r\n\r\n" +
                   "<!-- knt-ai:user -->\r\n**User:** \r\nQuestion\r\n\r\n" +
                   "<!-- knt-ai:assistant {not json} -->\r\n**Assistant:** \r\nAnswer\r\n";

        var parsed = AiChatSessionTranscript.Parse(text);

        Assert.AreEqual(1, parsed.Count);
        Assert.AreEqual("Question", parsed[0].Prompt);
        Assert.AreEqual("Answer", parsed[0].Answer);
        Assert.AreEqual(0, parsed[0].TotalTokens);
    }

    [TestMethod]
    public void Parse_EmptyOrPlainText_ReturnsNoTurns()
    {
        Assert.AreEqual(0, AiChatSessionTranscript.Parse(null).Count);
        Assert.AreEqual(0, AiChatSessionTranscript.Parse("").Count);
        Assert.AreEqual(0, AiChatSessionTranscript.Parse("**User:** hi\r\n**Assistant:** hello").Count);
    }
}
