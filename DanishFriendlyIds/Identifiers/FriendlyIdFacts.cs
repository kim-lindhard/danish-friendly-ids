using Xunit;

namespace DanishFriendlyIds.Identifiers;

public class FriendlyIdFacts
{
    [Fact]
    public void An_identifier_reads_as_adjective_space_noun()
    {
        // Arrange
        var id = new FriendlyId("glade", "danser");

        // Act
        var text = id.ToString();

        // Assert
        Assert.Equal("glade danser", text);
    }

    [Fact]
    public void Two_words_parse_back_into_an_identifier()
    {
        // Act
        var parsed = FriendlyId.TryParse("trætte cyklist", out var id);

        // Assert
        Assert.True(parsed);
        Assert.Equal(new FriendlyId("trætte", "cyklist"), id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("glade")]
    [InlineData("glade  danser")]
    [InlineData("den glade danser")]
    public void Anything_but_two_words_does_not_parse(string? text)
    {
        // Act
        var parsed = FriendlyId.TryParse(text, out var id);

        // Assert
        Assert.False(parsed);
        Assert.Null(id);
    }
}
