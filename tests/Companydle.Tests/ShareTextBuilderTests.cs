using Companydle.Models;
using Companydle.Services;

namespace Companydle.Tests;

public class ShareTextBuilderTests
{
    [Test]
    public void Build_IncludesHeaderRowsAndUrl()
    {
        var guesses = new List<GuessResult>
        {
            new() { Guess = new Company { Id = "one", Name = "One" }, Attributes = Attributes(Feedback.Correct) },
            new() { Guess = new Company { Id = "two", Name = "Two" }, Attributes = Attributes(Feedback.Wrong) },
        };

        var text = ShareTextBuilder.Build(7, true, guesses, "https://example.test/companydle/");
        var lines = text.Split('\n');

        Assert.Multiple(() =>
        {
            Assert.That(lines[0], Is.EqualTo("Companydle #7 2/8"));
            Assert.That(lines[2], Is.EqualTo("🟩🟩🟩🟩🟩🟩🟩🟩🟩🟩"));
            Assert.That(lines[3], Is.EqualTo("⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛"));
            Assert.That(lines[^1], Is.EqualTo("https://example.test/companydle/"));
        });
    }

    [Test]
    public void Build_UsesXScoreForLossAndArchiveTitle()
    {
        var guesses = new List<GuessResult>
        {
            new() { Guess = new Company { Id = "one", Name = "One" }, Attributes = Attributes(Feedback.Close) },
        };

        var text = ShareTextBuilder.Build(0, false, guesses, "url", isArchive: true);

        Assert.That(text.Split('\n')[0], Is.EqualTo("Companydle Archive #0 X/8"));
        Assert.That(text.Split('\n')[2], Is.EqualTo("🟨🟨🟨🟨🟨🟨🟨🟨🟨🟨"));
    }

    private static List<AttributeResult> Attributes(Feedback feedback) =>
        Enumerable.Range(0, 10)
            .Select(index => new AttributeResult
            {
                Kind = (AttributeKind)index,
                Label = "Tile",
                DisplayValue = "value",
                Feedback = feedback,
            })
            .ToList();
}
