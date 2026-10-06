using Xunit;

namespace DanishFriendlyIds.Words;

public class LexiconFacts
{
    private static readonly Lexicon Lexicon = Lexicon.Embedded;

    private static Word Only(string lemma, WordClass wordClass) =>
        Lexicon.Find(lemma).Single(word => word.WordClass == wordClass);

    [Fact]
    public void The_embedded_lexicon_holds_every_headword_of_cor_and_cor_ext()
    {
        // Act
        var words = Lexicon.Words;

        // Assert
        Assert.Equal(94_047, words.Count);
        Assert.Equal(93_811, words.Select(word => word.Id).Distinct().Count());
    }

    [Theory]
    [InlineData("sb", 69_514)]
    [InlineData("adj", 13_315)]
    [InlineData("vb", 7_556)]
    [InlineData("adv", 941)]
    [InlineData("prop", 683)]
    [InlineData("fork", 561)]
    [InlineData("infmærke", 1)]
    public void Each_word_class_has_the_count_published_in_cor(string corLabel, int expectedCount)
    {
        // Arrange
        Assert.True(WordClass.TryFromCorLabel(corLabel, out var wordClass));

        // Act
        var count = Lexicon.Of(wordClass).Count();

        // Assert
        Assert.Equal(expectedCount, count);
    }

    [Theory]
    [InlineData("glad", "glade")]
    [InlineData("træt", "trætte")]
    [InlineData("kold", "kolde")]
    [InlineData("blå", "blå")]
    [InlineData("moderne", "moderne")]
    public void An_adjective_carries_its_definite_form(string lemma, string expectedDefiniteForm)
    {
        // Act
        var adjective = Only(lemma, WordClass.Adjective);

        // Assert
        Assert.Equal(expectedDefiniteForm, adjective.DefiniteForm);
    }

    [Theory]
    [InlineData("danser")]
    [InlineData("cyklist")]
    public void A_person_noun_is_human(string lemma)
    {
        // Act
        var person = Only(lemma, WordClass.Noun);

        // Assert
        Assert.True(person.Is(MeaningCategory.Human));
    }

    [Fact]
    public void A_box_is_a_container_artifact_and_object()
    {
        // Act
        var box = Only("kasse", WordClass.Noun);

        // Assert
        Assert.True(box.Is(MeaningCategory.Container));
        Assert.True(box.Is(MeaningCategory.Artifact));
        Assert.True(box.Is(MeaningCategory.Object));
        Assert.False(box.Is(MeaningCategory.Human));
    }

    [Fact]
    public void Happy_is_a_mental_property()
    {
        // Act
        var happy = Only("glad", WordClass.Adjective);

        // Assert
        Assert.True(happy.Is(MeaningCategory.Mental));
        Assert.True(happy.MinimumSentiment > 0);
    }

    [Fact]
    public void A_headword_under_two_classes_is_two_words()
    {
        // Act
        var accordingTo = Lexicon.Find("ifølge").Select(word => word.WordClass).ToHashSet();

        // Assert
        Assert.Equal(new HashSet<WordClass> { WordClass.Preposition, WordClass.Abbreviation }, accordingTo);
    }

    [Fact]
    public void A_word_keeps_every_one_of_its_senses()
    {
        // Act
        var box = Only("kasse", WordClass.Noun);

        // Assert
        Assert.Equal(9, box.Senses.Count);
    }

    [Fact]
    public void Red_has_a_colour_sense_that_is_not_a_condition()
    {
        // Act
        var red = Only("rød", WordClass.Adjective);

        // Assert
        Assert.Contains(red.Senses, sense => sense.Is(MeaningCategory.Colour) && sense.Is(MeaningCategory.Condition) == false);
        Assert.Contains(red.Senses, sense => sense.Is(MeaningCategory.Condition));
    }

    [Fact]
    public void The_embedded_lexicon_holds_every_sense_the_builder_attached()
    {
        // Act
        var senses = Lexicon.Words.Sum(word => word.Senses.Count);

        // Assert
        Assert.Equal(42_711, senses);
    }

    [Fact]
    public void About_thirty_four_thousand_headwords_have_a_meaning()
    {
        // Act
        var headwordsWithMeaning = Lexicon.Words
            .Where(word => word.HasMeaning)
            .Select(word => word.Id)
            .Distinct()
            .Count();

        // Assert
        Assert.InRange(headwordsWithMeaning, 33_500, 34_500);
    }
}
