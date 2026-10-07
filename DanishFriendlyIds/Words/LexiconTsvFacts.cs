using Xunit;

namespace DanishFriendlyIds.Words;

public class LexiconTsvFacts
{
    private static WordSense Sense(int? sentiment, int? centrality, params MeaningCategory[] categories) =>
        new(categories.ToHashSet(), new HashSet<Topic>(), sentiment, centrality, new HashSet<Restriction>());

    private static readonly Word Cold = new(
        new WordId("COR.15653"),
        "kold",
        WordClass.Adjective,
        "kolde",
        [
            Sense(null, 3, MeaningCategory.Property, MeaningCategory.Physical),
            Sense(-2, 3, MeaningCategory.Property, MeaningCategory.Mental) with
            {
                Topics = new HashSet<Topic> { new("psy") },
                Restrictions = new HashSet<Restriction> { Restriction.Usage }
            }
        ],
        new HashSet<Restriction>());

    private static readonly Word Table = new(
        new WordId("COR.44636"),
        "bord",
        WordClass.Noun,
        null,
        [],
        new HashSet<Restriction>());

    private static (string Words, string Senses) Written(params Word[] words)
    {
        using var wordsWriter = new StringWriter();
        using var sensesWriter = new StringWriter();
        LexiconTsv.Write(words, wordsWriter, sensesWriter);
        return (wordsWriter.ToString(), sensesWriter.ToString());
    }

    private static IReadOnlyList<Word> ReadBack((string Words, string Senses) files) =>
        LexiconTsv.Read(new StringReader(files.Words), new StringReader(files.Senses));

    [Fact]
    public void A_word_line_summarises_its_senses()
    {
        // Act
        var (words, _) = Written(Cold);

        // Assert
        Assert.Equal(
            "id\tlemma\tword_class\tdefinite_form\tcategories\ttopics\tmin_sentiment\tcentrality\trestriction\tsenses\tpresent_participle\n" +
            "COR.15653\tkold\tadj\tkolde\tMental|Physical|Property\tpsy\t-2\t3\t\t2\t\n",
            words);
    }

    [Fact]
    public void Each_sense_gets_its_own_numbered_line()
    {
        // Act
        var (_, senses) = Written(Cold, Table);

        // Assert
        Assert.Equal(
            "id\tword_class\tsense\tcategories\ttopics\tsentiment\tcentrality\trestriction\n" +
            "COR.15653\tadj\t1\tPhysical|Property\t\t\t3\t\n" +
            "COR.15653\tadj\t2\tMental|Property\tpsy\t-2\t3\tsprogbrug\n",
            senses);
    }

    [Fact]
    public void Words_and_their_senses_read_back_equal_to_what_was_written()
    {
        // Act
        var words = ReadBack(Written(Cold, Table));

        // Assert
        Assert.Equal(2, words.Count);
        var cold = words[0];
        Assert.Equal("kolde", cold.DefiniteForm);
        Assert.Equal(2, cold.Senses.Count);
        Assert.False(cold.Senses[0].Is(MeaningCategory.Mental));
        Assert.True(cold.Senses[1].Is(MeaningCategory.Mental));
        Assert.Equal(-2, cold.Senses[1].Sentiment);
        Assert.Equal(new HashSet<Restriction> { Restriction.Usage }, cold.Senses[1].Restrictions);
        Assert.Empty(cold.Restrictions);
        Assert.Equal(-2, cold.MinimumSentiment);
        Assert.Equal(3, cold.Centrality);
        var table = words[1];
        Assert.Equal(WordClass.Noun, table.WordClass);
        Assert.Null(table.DefiniteForm);
        Assert.False(table.HasMeaning);
        Assert.Null(table.Centrality);
    }

    [Fact]
    public void A_words_file_with_another_header_is_rejected()
    {
        // Arrange
        var (_, senses) = Written(Cold);

        // Act
        var error = Assert.Throws<InvalidDataException>(() => ReadBack(("id\tlemma\n", senses)));

        // Assert
        Assert.Contains("words header", error.Message);
    }

    [Fact]
    public void A_word_whose_senses_are_missing_is_rejected()
    {
        // Arrange
        var (words, _) = Written(Cold);
        var emptySenses = Written().Senses;

        // Act
        var error = Assert.Throws<InvalidDataException>(() => ReadBack((words, emptySenses)));

        // Assert
        Assert.Contains("2 senses expected, 0 found", error.Message);
    }

    [Fact]
    public void An_unknown_word_class_is_rejected_with_its_line_number()
    {
        // Arrange
        var (words, senses) = Written(Table);

        // Act
        var error = Assert.Throws<InvalidDataException>(() => ReadBack((words.Replace("\tsb\t", "\tnoget\t"), senses)));

        // Assert
        Assert.Contains("words line 2", error.Message);
    }
}
