namespace Companydle.Tests;

public class StableHashTests
{
    [TestCase("", 2166136261u)]
    [TestCase("a", 3826002220u)]
    [TestCase("foobar", 3214735720u)]
    public void Fnv1a32_MatchesKnownVectors(string input, uint expected)
    {
        Assert.That(StableHash.Fnv1a32(input), Is.EqualTo(expected));
    }
}
