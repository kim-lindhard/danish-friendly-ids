using DanishFriendlyIds.Words;
using Xunit;

namespace DanishFriendlyIds.Identifiers;

public class WordPoolSelectorFacts
{
    private static readonly WordPools Person = WordPoolSelector.Select(Lexicon.Embedded, ReviewLists.Embedded, IdKind.Person);
    private static readonly WordPools Object = WordPoolSelector.Select(Lexicon.Embedded, ReviewLists.Embedded, IdKind.Object);

    private static IEnumerable<string> EveryPooledWord =>
        Person.Adjectives.Concat(Person.Nouns).Concat(Object.Adjectives).Concat(Object.Nouns);

    [Theory]
    [InlineData("glade")]
    [InlineData("modige")]
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
    public void Object_adjectives_include_everyday_colours_shapes_and_sizes(string adjective) =>
        Assert.Contains(adjective, Object.Adjectives);

    [Theory]
    [InlineData("kasse")]
    [InlineData("bord")]
    public void Object_nouns_include_the_examples(string noun) =>
        Assert.Contains(noun, Object.Nouns);

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
    public void Negative_and_sensitive_adjectives_are_never_used_for_an_object(string adjective) =>
        Assert.DoesNotContain(adjective, Object.Adjectives);

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
    public void Weapons_grim_places_and_words_that_can_name_a_person_are_never_used_for_an_object(string noun) =>
        Assert.DoesNotContain(noun, Object.Nouns);

    [Fact]
    public void Every_pooled_word_is_approved_in_its_own_list()
    {
        // Arrange
        var people = ReviewLists.Embedded.People;
        var objects = ReviewLists.Embedded.Objects;

        // Act
        var unapproved = Person.Adjectives.Where(word => people.IsApproved(WordClass.Adjective, word) == false)
            .Concat(Person.Nouns.Where(word => people.IsApproved(WordClass.Noun, word) == false))
            .Concat(Object.Adjectives.Where(word => objects.IsApproved(WordClass.Adjective, word) == false))
            .Concat(Object.Nouns.Where(word => objects.IsApproved(WordClass.Noun, word) == false))
            .ToList();

        // Assert
        Assert.Empty(unapproved);
    }

    [Theory]
    [InlineData("adrætte")]
    [InlineData("balancerede")]
    [InlineData("bekendte")]
    [InlineData("centrale")]
    [InlineData("distraherede")]
    [InlineData("dynamiske")]
    [InlineData("energiske")]
    public void Adjectives_kim_placed_with_people_describe_people_only(string adjective)
    {
        Assert.Contains(adjective, Person.Adjectives);
        Assert.DoesNotContain(adjective, Object.Adjectives);
    }

    [Theory]
    [InlineData("antikke")]
    [InlineData("appetitlige")]
    [InlineData("bevægelige")]
    [InlineData("byggede")]
    [InlineData("børstede")]
    [InlineData("dobbelte")]
    [InlineData("driftsklare")]
    [InlineData("dybe")]
    [InlineData("elektriske")]
    [InlineData("elektroniske")]
    public void Adjectives_kim_placed_with_objects_describe_objects_only(string adjective)
    {
        Assert.Contains(adjective, Object.Adjectives);
        Assert.DoesNotContain(adjective, Person.Adjectives);
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
    public void Every_object_adjective_has_a_fitting_sense_that_is_not_negative()
    {
        // Arrange
        var adjectivesByForm = Lexicon.Embedded.Of(WordClass.Adjective)
            .Where(word => word.DefiniteForm is not null)
            .ToLookup(word => word.DefiniteForm);

        // Act
        var withoutAFittingSense = Object.Adjectives
            .Where(form => adjectivesByForm[form].Any(IdKind.Object.Adjectives.MatchesASenseOf) == false)
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
        Assert.Equal(Object.Adjectives.Distinct().Order(StringComparer.Ordinal), Object.Adjectives);
    }
}
