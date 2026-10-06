using DanishFriendlyIds.Words;
using Xunit;

namespace DanishFriendlyIds.Identifiers;

public class WordPoolSelectorFacts
{
    private static readonly WordPools Person = WordPoolSelector.Select(Lexicon.Embedded, Blocklist.Embedded, IdKind.Person);
    private static readonly WordPools Case = WordPoolSelector.Select(Lexicon.Embedded, Blocklist.Embedded, IdKind.Case);

    [Theory]
    [InlineData("glade")]
    [InlineData("trætte")]
    public void Person_adjectives_include_the_examples(string adjective) =>
        Assert.Contains(adjective, Person.Adjectives);

    [Theory]
    [InlineData("danser")]
    [InlineData("cyklist")]
    [InlineData("bager")]
    [InlineData("ambassadør")]
    public void Person_nouns_include_people_even_when_another_sense_is_a_group_or_institution(string noun) =>
        Assert.Contains(noun, Person.Nouns);

    [Theory]
    [InlineData("band")]
    [InlineData("befolkning")]
    public void Person_nouns_leave_out_words_that_only_name_groups(string noun) =>
        Assert.DoesNotContain(noun, Person.Nouns);

    [Theory]
    [InlineData("kolde")]
    [InlineData("sorte")]
    [InlineData("snobbede")]
    [InlineData("pedantiske")]
    public void Person_adjectives_leave_out_negative_sensitive_or_restricted_words(string adjective) =>
        Assert.DoesNotContain(adjective, Person.Adjectives);

    [Theory]
    [InlineData("jøde")]
    [InlineData("roma")]
    [InlineData("dame")]
    [InlineData("bøsse")]
    public void Person_nouns_leave_out_sensitive_or_restricted_words(string noun) =>
        Assert.DoesNotContain(noun, Person.Nouns);

    [Theory]
    [InlineData("kolde")]
    [InlineData("røde")]
    [InlineData("blå")]
    public void Case_adjectives_include_colours_and_physical_properties(string adjective) =>
        Assert.Contains(adjective, Case.Adjectives);

    [Theory]
    [InlineData("kasse")]
    [InlineData("bord")]
    public void Case_nouns_include_concrete_things(string noun) =>
        Assert.Contains(noun, Case.Nouns);

    [Fact]
    public void Case_nouns_leave_out_people()
    {
        Assert.DoesNotContain("danser", Case.Nouns);
        Assert.DoesNotContain("bøsse", Case.Nouns);
    }

    [Fact]
    public void Every_pooled_word_is_three_to_twelve_lowercase_danish_letters()
    {
        // Arrange
        var everyPooledWord = Person.Adjectives.Concat(Person.Nouns).Concat(Case.Adjectives).Concat(Case.Nouns);

        // Act
        var malformed = everyPooledWord.Where(word => WordPoolSelector.HasIdentifierShape(word) == false).ToList();

        // Assert
        Assert.Empty(malformed);
    }

    [Fact]
    public void Pools_are_distinct_and_in_ordinal_order()
    {
        Assert.Equal(Person.Nouns.Distinct().Order(StringComparer.Ordinal), Person.Nouns);
        Assert.Equal(Case.Adjectives.Distinct().Order(StringComparer.Ordinal), Case.Adjectives);
    }
}
