using DanishFriendlyIds.Words;
using Xunit;

namespace DanishFriendlyIds.WordListBuilder;

public class CorSemReaderFacts
{
    [Fact]
    public void A_sense_pointing_at_two_cor_headwords_gets_both_as_targets()
    {
        // Arrange
        var senses = Fixtures.CorSemSenses();

        // Act
        var firstSenseOfA = senses.First(sense => sense.Lemma == "A");

        // Assert
        Assert.Equal([new WordId("COR.51220"), new WordId("COR.45519")], firstSenseOfA.Targets);
    }

    [Fact]
    public void A_sense_pointing_at_cor_ext_gets_the_ext_id_as_its_target()
    {
        // Arrange
        var senses = Fixtures.CorSemSenses();

        // Act
        var thirteen = senses.Single(sense => sense.Lemma == "13-tal");

        // Assert
        Assert.Equal([new WordId("COR.EXT.137129")], thirteen.Targets);
    }

    [Fact]
    public void An_ontological_type_with_alternatives_is_split_into_all_its_atoms()
    {
        // Arrange
        var senses = Fixtures.CorSemSenses();

        // Act
        var coldAsAPersonality = senses.Where(sense => sense.Lemma == "kold").ElementAt(2);

        // Assert
        Assert.Equal(
            new HashSet<MeaningCategory> { MeaningCategory.Property, MeaningCategory.Mental, MeaningCategory.Social },
            coldAsAPersonality.Categories);
        Assert.Equal(-2, coldAsAPersonality.Sentiment);
        Assert.Equal(3, coldAsAPersonality.Centrality);
        Assert.Equal(new HashSet<Restriction> { Restriction.Usage }, coldAsAPersonality.Restrictions);
    }

    [Fact]
    public void The_ddo_word_class_maps_onto_the_cor_label()
    {
        // Arrange
        var sense = Fixtures.CorSemSenses().First(candidate => candidate.Lemma == "glad");

        // Act
        var corLabel = sense.CorWordClassLabel;

        // Assert
        Assert.Equal("adj.", sense.DdoWordClass);
        Assert.Equal("adj", corLabel);
    }
}
