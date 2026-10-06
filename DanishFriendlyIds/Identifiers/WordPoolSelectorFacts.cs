using DanishFriendlyIds.Words;
using Xunit;

namespace DanishFriendlyIds.Identifiers;

public class WordPoolSelectorFacts
{
    private static readonly WordPools Person = WordPoolSelector.Select(Lexicon.Embedded, WordReview.Embedded, IdKind.Person);
    private static readonly WordPools Case = WordPoolSelector.Select(Lexicon.Embedded, WordReview.Embedded, IdKind.Case);

    private static IEnumerable<string> EveryPooledWord =>
        Person.Adjectives.Concat(Person.Nouns).Concat(Case.Adjectives).Concat(Case.Nouns);

    [Theory]
    [InlineData("glade")]
    public void Person_adjectives_include_the_examples(string adjective) =>
        Assert.Contains(adjective, Person.Adjectives);

    [Theory]
    [InlineData("danser")]
    [InlineData("cyklist")]
    [InlineData("bager")]
    public void Person_nouns_include_people_even_when_another_sense_is_a_group_or_institution(string noun) =>
        Assert.Contains(noun, Person.Nouns);

    [Theory]
    [InlineData("blå")]
    [InlineData("runde")]
    [InlineData("store")]
    public void Case_adjectives_include_everyday_colours_shapes_and_sizes(string adjective) =>
        Assert.Contains(adjective, Case.Adjectives);

    [Theory]
    [InlineData("kasse")]
    [InlineData("bord")]
    public void Case_nouns_include_the_examples(string noun) =>
        Assert.Contains(noun, Case.Nouns);

    [Theory]
    [InlineData("vrede")]
    [InlineData("ildelugtende")]
    [InlineData("trætte")]
    [InlineData("kolde")]
    [InlineData("beskidte")]
    [InlineData("grimme")]
    [InlineData("døde")]
    [InlineData("sorte")]
    [InlineData("mørkhudede")]
    [InlineData("barmfagre")]
    public void Negative_and_sensitive_adjectives_are_never_used_for_a_person(string adjective) =>
        Assert.DoesNotContain(adjective, Person.Adjectives);

    [Theory]
    [InlineData("ildelugtende")]
    [InlineData("beskidte")]
    [InlineData("grimme")]
    [InlineData("døde")]
    [InlineData("rådne")]
    [InlineData("mørkhudede")]
    [InlineData("barmfagre")]
    public void Negative_and_sensitive_adjectives_are_never_used_for_a_case(string adjective) =>
        Assert.DoesNotContain(adjective, Case.Adjectives);

    [Theory]
    [InlineData("tyv")]
    [InlineData("taber")]
    [InlineData("idiot")]
    [InlineData("band")]
    [InlineData("jøde")]
    [InlineData("dame")]
    [InlineData("bøsse")]
    [InlineData("fange")]
    public void Negative_sensitive_or_group_nouns_are_never_used_for_a_person(string noun) =>
        Assert.DoesNotContain(noun, Person.Nouns);

    [Theory]
    [InlineData("gevær")]
    [InlineData("atomvåben")]
    [InlineData("dødscelle")]
    [InlineData("danser")]
    [InlineData("bager")]
    [InlineData("frisør")]
    [InlineData("købmand")]
    public void Weapons_grim_places_and_words_that_can_name_a_person_are_never_used_for_a_case(string noun) =>
        Assert.DoesNotContain(noun, Case.Nouns);

    [Fact]
    public void Every_pooled_word_is_approved()
    {
        // Arrange
        var review = WordReview.Embedded;

        // Act
        var unapproved = Person.Adjectives.Concat(Case.Adjectives).Where(word => review.IsApproved(WordClass.Adjective, word) == false)
            .Concat(Person.Nouns.Concat(Case.Nouns).Where(word => review.IsApproved(WordClass.Noun, word) == false))
            .ToList();

        // Assert
        Assert.Empty(unapproved);
    }

    [Fact]
    public void Every_person_word_has_no_negative_sense()
    {
        // Arrange
        var personWords = Lexicon.Embedded.Words
            .Where(word => word.WordClass == WordClass.Adjective && word.DefiniteForm is { } form && Person.Adjectives.Contains(form)
                           || word.WordClass == WordClass.Noun && Person.Nouns.Contains(word.Lemma))
            .Where(word => WordPoolSelector.FollowsTheRules(word, IdKind.Person));

        // Act
        var negative = personWords.Where(word => word.Senses.Any(sense => sense.Sentiment < 0)).Select(word => word.Lemma).ToList();

        // Assert
        Assert.Empty(negative);
    }

    [Fact]
    public void Every_case_adjective_has_a_fitting_sense_that_is_not_negative()
    {
        // Arrange
        var adjectivesByForm = Lexicon.Embedded.Of(WordClass.Adjective)
            .Where(word => word.DefiniteForm is not null)
            .ToLookup(word => word.DefiniteForm);

        // Act
        var withoutAFittingSense = Case.Adjectives
            .Where(form => adjectivesByForm[form].Any(IdKind.Case.Adjectives.MatchesASenseOf) == false)
            .ToList();

        // Assert
        Assert.Empty(withoutAFittingSense);
    }

    [Fact]
    public void Every_pooled_word_is_three_to_twelve_lowercase_danish_letters()
    {
        // Act
        var malformed = EveryPooledWord.Where(word => WordPoolSelector.HasIdentifierShape(word) == false).ToList();

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
