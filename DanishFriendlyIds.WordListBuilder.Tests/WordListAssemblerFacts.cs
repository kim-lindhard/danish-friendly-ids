using DanishFriendlyIds.Words;
using Xunit;

namespace DanishFriendlyIds.WordListBuilder;

public class WordListAssemblerFacts
{
    private static readonly AssembledWordList Assembled = Fixtures.Assembled();

    private static Word Only(string lemma, WordClass wordClass) =>
        Assembled.Words.Single(word => word.Lemma == lemma && word.WordClass == wordClass);

    [Theory]
    [InlineData("glad", "glade")]
    [InlineData("træt", "trætte")]
    [InlineData("kold", "kolde")]
    [InlineData("blå", "blå")]
    [InlineData("moderne", "moderne")]
    [InlineData("pladderhumanistisk", "pladderhumanistiske")]
    public void An_adjective_gets_its_definite_form(string lemma, string expectedDefiniteForm)
    {
        // Act
        var adjective = Only(lemma, WordClass.Adjective);

        // Assert
        Assert.Equal(expectedDefiniteForm, adjective.DefiniteForm);
    }

    [Fact]
    public void A_verb_gets_its_present_participle_and_other_words_get_none()
    {
        // Act
        var dance = Only("danse", WordClass.Verb);
        var happy = Only("glad", WordClass.Adjective);
        var box = Only("kasse", WordClass.Noun);

        // Assert
        Assert.Equal("dansende", dance.PresentParticiple);
        Assert.True(dance.Is(MeaningCategory.Act));
        Assert.Null(happy.PresentParticiple);
        Assert.Null(box.PresentParticiple);
    }

    [Fact]
    public void A_noun_has_no_definite_form()
    {
        // Act
        var box = Only("kasse", WordClass.Noun);

        // Assert
        Assert.Null(box.DefiniteForm);
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
    public void A_word_gets_the_categories_and_topics_of_all_its_senses()
    {
        // Act
        var box = Only("kasse", WordClass.Noun);

        // Assert
        Assert.Equal(9, box.Senses.Count);
        Assert.True(box.Is(MeaningCategory.Container));
        Assert.True(box.Is(MeaningCategory.Artifact));
        Assert.True(box.Is(MeaningCategory.Object));
        Assert.True(box.Is(MeaningCategory.MoneyRepresentation));
        Assert.Equal(new HashSet<Topic> { new("øko"), new("mat"), new("typ") }, box.Topics);
        Assert.Equal(3, box.Centrality);
    }

    [Fact]
    public void Each_sense_keeps_its_own_categories_sentiment_and_restrictions()
    {
        // Act
        var cold = Only("kold", WordClass.Adjective);

        // Assert
        Assert.Equal(3, cold.Senses.Count);
        Assert.True(cold.Senses[0].Is(MeaningCategory.Physical));
        Assert.False(cold.Senses[0].Is(MeaningCategory.Mental));
        Assert.True(cold.Senses[1].Is(MeaningCategory.Colour));
        Assert.True(cold.Senses[2].Is(MeaningCategory.Mental));
        Assert.Equal(-2, cold.Senses[2].Sentiment);
        Assert.Equal(new HashSet<Restriction> { Restriction.Usage }, cold.Senses[2].Restrictions);
        Assert.Null(cold.Senses[0].Sentiment);
    }

    [Fact]
    public void A_restriction_on_only_some_senses_does_not_restrict_the_word()
    {
        // Act
        var box = Only("kasse", WordClass.Noun);
        var cold = Only("kold", WordClass.Adjective);

        // Assert
        Assert.Empty(box.Restrictions);
        Assert.Empty(cold.Restrictions);
    }

    [Fact]
    public void A_word_keeps_the_lowest_sentiment_of_its_senses()
    {
        // Act
        var cold = Only("kold", WordClass.Adjective);
        var tired = Only("træt", WordClass.Adjective);
        var happy = Only("glad", WordClass.Adjective);

        // Assert
        Assert.Equal(-2, cold.MinimumSentiment);
        Assert.Equal(-1, tired.MinimumSentiment);
        Assert.Equal(3, happy.MinimumSentiment);
    }

    [Fact]
    public void A_registered_trademark_is_restricted()
    {
        // Act
        var trademarked = Assembled.Words.Single(word => word.Id == new WordId("COR.EXT.145381"));

        // Assert
        Assert.Contains(Restriction.Trademark, trademarked.Restrictions);
    }

    [Fact]
    public void A_headword_under_two_classes_becomes_two_words_with_one_id()
    {
        // Act
        var accordingTo = Assembled.Words.Where(word => word.Lemma == "ifølge").ToList();

        // Assert
        Assert.Equal(2, accordingTo.Count);
        Assert.Single(accordingTo.Select(word => word.Id).Distinct());
        Assert.Contains(accordingTo, word => word.WordClass == WordClass.Preposition);
        Assert.Contains(accordingTo, word => word.WordClass == WordClass.Abbreviation);
    }

    [Fact]
    public void A_sense_shared_by_two_cor_headwords_gives_both_their_meaning()
    {
        // Act
        var upperCase = Assembled.Words.Single(word => word.Id == new WordId("COR.51220"));
        var lowerCase = Assembled.Words.Single(word => word.Id == new WordId("COR.45519"));

        // Assert
        Assert.Equal(2, upperCase.Senses.Count);
        Assert.Equal(2, lowerCase.Senses.Count);
    }

    [Fact]
    public void A_word_outside_cor_sem_keeps_its_class_and_has_no_meaning()
    {
        // Act
        var tables = Assembled.Words.Where(word => word.Lemma == "bord").ToList();

        // Assert
        Assert.Equal(2, tables.Count);
        Assert.All(tables, table => Assert.Equal(WordClass.Noun, table.WordClass));
        Assert.All(tables, table => Assert.False(table.HasMeaning));
        Assert.All(tables, table => Assert.Empty(table.Categories));
    }

    [Fact]
    public void A_sense_without_a_link_falls_back_to_the_headword_with_the_same_lemma_and_class()
    {
        // Act
        var afterwardsAsAdverb = Only("bagefter", WordClass.Adverb);
        var afterwardsAsPreposition = Only("bagefter", WordClass.Preposition);

        // Assert
        Assert.Equal(3, afterwardsAsAdverb.Senses.Count);
        Assert.True(afterwardsAsAdverb.Is(MeaningCategory.TimeAdverb));
        Assert.False(afterwardsAsPreposition.HasMeaning);
    }

    [Fact]
    public void A_sense_whose_headword_is_missing_is_reported_as_unmatched()
    {
        // Act
        var unmatched = Assembled.UnmatchedSenses;

        // Assert
        Assert.Equal(["1. g'er"], unmatched.Select(sense => sense.Lemma));
    }

    [Fact]
    public void Words_are_ordered_by_id_then_class()
    {
        // Act
        var ids = Assembled.Words.Select(word => word.Id.Value).ToList();

        // Assert
        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
    }
}
