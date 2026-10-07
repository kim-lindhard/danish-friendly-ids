using System.Text.RegularExpressions;
using DanishFriendlyIds.Words;
using Xunit;
using static DanishFriendlyIds.TestSupport.TestWords;

namespace DanishFriendlyIds.Identifiers;

/// <summary>Danish, ASCII and URL-slug spellings, and resolving any of them back to the Danish identifier.</summary>
public partial class StyleFacts
{
    private static readonly FriendlyId Clever = FriendlyId.Of("kløgtige", "dansende", "pilot").WithNumber(42);

    [Theory]
    [InlineData("danish", "kløgtige dansende pilot 42")]
    [InlineData("ascii", "kloegtige dansende pilot 42")]
    [InlineData("url", "kloegtige-dansende-pilot-42")]
    public void An_identifier_is_written_in_the_chosen_style(string styleName, string expected)
    {
        // Arrange
        Assert.True(IdStyle.TryFromName(styleName, out var style));

        // Act
        var text = Clever.ToString(style);

        // Assert
        Assert.Equal(expected, text);
    }

    [Theory]
    [InlineData("blå", "blaa")]
    [InlineData("grågrønne", "graagroenne")]
    [InlineData("ærlige", "aerlige")]
    [InlineData("glade", "glade")]
    public void Folding_writes_ae_oe_and_aa(string word, string expected) =>
        Assert.Equal(expected, IdStyle.Fold(word));

    [Theory]
    [InlineData("kloegtige-dansende-pilot-42", 3, 42)]
    [InlineData("glade-danser", 2, null)]
    [InlineData("blaa kasse 7", 2, 7)]
    public void Slugs_and_ascii_text_parse(string text, int wordCount, int? number)
    {
        // Act
        var parsed = FriendlyId.TryParse(text, out var id);

        // Assert
        Assert.True(parsed);
        Assert.NotNull(id);
        Assert.Equal(wordCount, id.Words.Count);
        Assert.Equal(number, id.Number);
    }

    [Theory]
    [InlineData("glade danser-7")]
    [InlineData("glade--danser")]
    [InlineData("-glade-danser")]
    public void Mixed_or_empty_separators_do_not_parse(string text) =>
        Assert.False(FriendlyId.TryParse(text, out _));

    [Fact]
    public void Every_slug_uses_only_characters_a_url_never_escapes()
    {
        // Arrange
        var ids = new FriendlyIdGenerator(new Random(3));
        IdFormat[] formats = [IdFormat.TwoWords, IdFormat.ThreeWords.WithNumber(99)];

        // Act
        var slugs = IdKind.PresetsWithWordChoices
            .SelectMany(kind => formats.SelectMany(format =>
                Enumerable.Range(0, 2_500).Select(_ => ids.Next(kind, format).ToString(IdStyle.UrlSlug))))
            .ToList();

        // Assert
        Assert.Equal(20_000, slugs.Count);
        Assert.All(slugs, slug => Assert.Matches(UrlSlugShape(), slug));
        Assert.All(slugs, slug => Assert.Equal(slug, Uri.EscapeDataString(slug)));
    }

    [Fact]
    public void Every_style_resolves_back_to_the_danish_identifier()
    {
        // Arrange
        var ids = new FriendlyIdGenerator(new Random(4));
        var drawn = IdKind.PresetsWithWordChoices
            .SelectMany(kind => new[] { IdFormat.TwoWords, IdFormat.ThreeWords.WithNumber(9) }
                .SelectMany(format => Enumerable.Range(0, 500).Select(_ => (Kind: kind, Id: ids.Next(kind, format)))))
            .ToList();

        // Act
        var unresolved = drawn
            .SelectMany(entry => IdStyle.All.Select(style => (entry.Kind, entry.Id, Text: entry.Id.ToString(style))))
            .Where(entry => ids.TryResolve(entry.Kind, entry.Text, out var resolved) == false || resolved != entry.Id)
            .Select(entry => entry.Text)
            .ToList();

        // Assert
        Assert.Empty(unresolved);
    }

    [Fact]
    public void A_folded_word_resolves_to_its_danish_spelling()
    {
        // Arrange
        var blue = Adjective("blå", "blå", Sense(MeaningCategory.Property, MeaningCategory.Colour));
        var ids = ApprovingAll(blue, Box);

        // Act
        var resolved = ids.TryResolve(IdKind.Object, "blaa-kasse-3", out var id);

        // Assert
        Assert.True(resolved);
        Assert.Equal(FriendlyId.Of("blå", "kasse").WithNumber(3), id);
    }

    [Theory]
    [InlineData("vrede-danser")]
    [InlineData("glade-traktor")]
    [InlineData("glade-dansende-dansende")]
    [InlineData("noget-helt-andet-her")]
    [InlineData("")]
    public void Text_this_kind_could_not_have_made_does_not_resolve(string text)
    {
        // Arrange
        var ids = ApprovingAll(Happy, Dancing, Dancer, Tractor);

        // Act
        var resolved = ids.TryResolve(IdKind.Person, text, out var id);

        // Assert
        Assert.False(resolved);
        Assert.Null(id);
    }

    [Fact]
    public void A_blocked_pair_does_not_resolve()
    {
        // Arrange
        var ids = PeopleListOnly([Approved(Happy), Approved(Dancer), BlockedPair("glade", "danser")], Happy, Dancer);

        // Act
        var resolved = ids.TryResolve(IdKind.Person, "glade-danser", out _);

        // Assert
        Assert.False(resolved);
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex UrlSlugShape();
}
