using Xunit;

namespace DanishFriendlyIds.Identifiers;

public class FriendlyIdFacts
{
    [Fact]
    public void An_identifier_reads_as_its_words_then_its_number()
    {
        // Arrange
        var twoWords = FriendlyId.Of("glade", "danser");
        var threeWordsAndNumber = FriendlyId.Of("glade", "dansende", "pilot").WithNumber(42);

        // Act
        var texts = new[] { twoWords.ToString(), threeWordsAndNumber.ToString() };

        // Assert
        Assert.Equal(["glade danser", "glade dansende pilot 42"], texts);
        Assert.Equal("pilot", threeWordsAndNumber.Noun);
    }

    [Theory]
    [InlineData("trætte cyklist", 2, null)]
    [InlineData("glade dansende pilot", 3, null)]
    [InlineData("blå kasse 7", 2, 7)]
    [InlineData("runde blinkende lampe 42", 3, 42)]
    public void Two_or_three_words_and_an_optional_number_parse_back(string text, int wordCount, int? number)
    {
        // Act
        var parsed = FriendlyId.TryParse(text, out var id);

        // Assert
        Assert.True(parsed);
        Assert.NotNull(id);
        Assert.Equal(wordCount, id.Words.Count);
        Assert.Equal(number, id.Number);
        Assert.Equal(text, id.ToString());
    }

    [Fact]
    public void Identifiers_with_the_same_words_and_number_are_equal()
    {
        Assert.Equal(FriendlyId.Of("glade", "danser"), FriendlyId.Of("glade", "danser"));
        Assert.Equal(FriendlyId.Of("glade", "danser").WithNumber(3), FriendlyId.Of("glade", "danser").WithNumber(3));
        Assert.NotEqual(FriendlyId.Of("glade", "danser"), FriendlyId.Of("glade", "danser").WithNumber(3));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("glade")]
    [InlineData("glade 7")]
    [InlineData("glade  danser")]
    [InlineData("den glade dansende pilot")]
    [InlineData("glade danser 0")]
    [InlineData("glade danser7")]
    public void Anything_but_two_or_three_words_and_a_positive_number_does_not_parse(string? text)
    {
        // Act
        var parsed = FriendlyId.TryParse(text, out var id);

        // Assert
        Assert.False(parsed);
        Assert.Null(id);
    }
}
