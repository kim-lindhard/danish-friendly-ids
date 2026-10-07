using System.Text.RegularExpressions;
using DanishFriendlyIds.Words;
using Xunit;
using static DanishFriendlyIds.TestSupport.TestWords;

namespace DanishFriendlyIds.Identifiers;

/// <summary>Danish, ASCII, URL-slug, PascalCase and camelCase spellings, and resolving any of them back to the Danish identifier.</summary>
public partial class StyleFacts
{
    private static readonly FriendlyId Clever = FriendlyId.Of("kløgtige", "dansende", "pilot").WithNumber(42);

    [Theory]
    [InlineData("danish", "kløgtige dansende pilot 42")]
    [InlineData("ascii", "kloegtige dansende pilot 42")]
    [InlineData("url", "kloegtige-dansende-pilot-42")]
    [InlineData("pascal", "KløgtigeDansendePilot42")]
    [InlineData("pascal-ascii", "KloegtigeDansendePilot42")]
    [InlineData("camel", "kløgtigeDansendePilot42")]
    [InlineData("camel-ascii", "kloegtigeDansendePilot42")]
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
    [InlineData("KloegtigeDansendePilot42", 3, 42)]
    [InlineData("gladeDanser", 2, null)]
    [InlineData("BlåKasse7", 2, 7)]
    public void Slugs_ascii_and_camel_case_text_parse(string text, int wordCount, int? number)
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
    [InlineData("GladeDanser-7")]
    public void Mixed_or_empty_separators_do_not_parse(string text) =>
        Assert.False(FriendlyId.TryParse(text, out _));

    [Theory]
    [InlineData("gladedanser")]
    [InlineData("GladeDANSER")]
    [InlineData("Glade42Danser")]
    [InlineData("Glade")]
    public void Joined_text_parses_only_when_capitals_mark_each_word(string text) =>
        Assert.False(FriendlyId.TryParse(text, out _));

    [Fact]
    public void Camel_case_text_parses_to_lowercase_words()
    {
        // Act
        var parsed = FriendlyId.TryParse("ØvedeDanser7", out var id);

        // Assert
        Assert.True(parsed);
        Assert.Equal(FriendlyId.Of("øvede", "danser").WithNumber(7), id);
    }

    [Theory]
    [InlineData("danish", "ascii")]
    [InlineData("ascii", "ascii")]
    [InlineData("url", "url")]
    [InlineData("pascal", "pascal-ascii")]
    [InlineData("camel", "camel-ascii")]
    public void Ascii_letters_turn_a_style_into_its_folded_twin(string styleName, string twinName)
    {
        // Arrange
        Assert.True(IdStyle.TryFromName(styleName, out var style));
        Assert.True(IdStyle.TryFromName(twinName, out var twin));

        // Act
        var folded = style.WithAsciiLetters();

        // Assert
        Assert.Equal(twin, folded);
    }

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
    public void Every_folded_camel_case_identifier_is_ascii_letters_and_digits()
    {
        // Arrange
        var ids = new FriendlyIdGenerator(new Random(5));
        IdFormat[] formats = [IdFormat.TwoWords, IdFormat.ThreeWords.WithNumber(99)];
        IdStyle[] styles = [IdStyle.PascalCase.WithAsciiLetters(), IdStyle.CamelCase.WithAsciiLetters()];

        // Act
        var texts = IdKind.PresetsWithWordChoices
            .SelectMany(kind => formats.SelectMany(format =>
                Enumerable.Range(0, 1_250).Select(_ => ids.Next(kind, format))))
            .SelectMany(id => styles.Select(id.ToString))
            .ToList();

        // Assert
        Assert.Equal(20_000, texts.Count);
        Assert.All(texts, text => Assert.Matches(FoldedCamelCaseShape(), text));
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

    [Fact]
    public void A_word_starting_with_ae_capitalises_and_resolves_in_both_spellings()
    {
        // Arrange
        var honest = Adjective("ærlig", "ærlige", Sense(MeaningCategory.Property, MeaningCategory.Mental));
        var ids = ApprovingAll(honest, Dancer);
        var expected = FriendlyId.Of("ærlige", "danser");

        // Act
        var danish = expected.ToString(IdStyle.PascalCase);
        var ascii = expected.ToString(IdStyle.PascalCase.WithAsciiLetters());

        // Assert
        Assert.Equal("ÆrligeDanser", danish);
        Assert.Equal("AerligeDanser", ascii);
        Assert.True(ids.TryResolve(IdKind.Person, danish, out var fromDanish));
        Assert.True(ids.TryResolve(IdKind.Person, ascii, out var fromAscii));
        Assert.Equal(expected, fromDanish);
        Assert.Equal(expected, fromAscii);
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

    [GeneratedRegex("^[A-Za-z][a-z]*([A-Z][a-z]+)+[0-9]*$")]
    private static partial Regex FoldedCamelCaseShape();
}
