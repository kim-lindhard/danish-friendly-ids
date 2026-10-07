using DanishFriendlyIds.Words;
using Xunit;
using static DanishFriendlyIds.TestSupport.TestWords;

namespace DanishFriendlyIds.Identifiers;

/// <summary>Which words a kind may draw: per-sense categories, per-word safety, and each kind's own list.</summary>
public class KindRulesFacts
{
    [Fact]
    public void A_word_qualifies_through_any_one_of_its_senses()
    {
        // Arrange
        var ids = ApprovingAll(Red, Flushed, Tractor);

        // Act
        var objectAdjectives = ids.PoolsOf(IdKind.Object).Adjectives;

        // Assert
        Assert.Equal(["røde"], objectAdjectives);
    }

    [Fact]
    public void A_word_approved_only_in_the_objects_list_is_never_drawn_for_a_person()
    {
        // Arrange
        var electric = Adjective("elektrisk", "elektriske",
            Sense(MeaningCategory.Property, MeaningCategory.Physical),
            Sense(MeaningCategory.Property, MeaningCategory.Mental));
        var ids = Generator(
            peopleList: [Approved(Happy), Approved(Dancer)],
            objectsList: [Approved(electric), Approved(Box)],
            Happy, electric, Dancer, Box);

        // Act
        var personAdjectives = ids.PoolsOf(IdKind.Person).Adjectives;
        var objectAdjectives = ids.PoolsOf(IdKind.Object).Adjectives;

        // Assert
        Assert.Equal(["glade"], personAdjectives);
        Assert.Equal(["elektriske"], objectAdjectives);
    }

    [Fact]
    public void A_physical_trait_can_describe_a_person()
    {
        // Arrange
        var energetic = Adjective("energisk", "energiske", Sense(MeaningCategory.Property, MeaningCategory.Physical) with { Sentiment = 2 });
        var ids = PeopleListOnly([Approved(energetic), Approved(Dancer)], energetic, Dancer);

        // Act
        var drawn = Drawn(ids, IdKind.Person);

        // Assert
        Assert.Equal([new FriendlyId("energiske", "danser")], drawn);
    }

    [Fact]
    public void A_noun_that_can_name_a_person_is_never_an_object_noun()
    {
        // Arrange
        var baker = Noun("bager",
            Sense(MeaningCategory.Human, MeaningCategory.Object, MeaningCategory.Occupation),
            Sense(MeaningCategory.Building, MeaningCategory.Artifact, MeaningCategory.Object));
        var ids = ApprovingAll(Happy, Red, baker, Box);

        // Act
        var objectNouns = ids.PoolsOf(IdKind.Object).Nouns;
        var personNouns = ids.PoolsOf(IdKind.Person).Nouns;

        // Assert
        Assert.Equal(["kasse"], objectNouns);
        Assert.Equal(["bager"], personNouns);
    }

    [Fact]
    public void A_person_word_with_one_sensitive_sense_is_left_out()
    {
        // Arrange
        var dancerWithReligiousSense = Noun("danser",
            Sense(MeaningCategory.Human, MeaningCategory.Object),
            Sense(MeaningCategory.Human) with { Topics = new HashSet<Topic> { new("rel") } });
        var ids = ApprovingAll(Happy, dancerWithReligiousSense, Cyclist);

        // Act
        var nouns = ids.PoolsOf(IdKind.Person).Nouns;

        // Assert
        Assert.Equal(["cyklist"], nouns);
    }

    [Fact]
    public void A_word_with_an_ethnicity_sense_is_left_out_of_every_kind()
    {
        // Arrange
        var redWithEthnicSense = Adjective("rød", "røde",
            Sense(MeaningCategory.Property, MeaningCategory.Colour),
            Sense(MeaningCategory.Property, MeaningCategory.Physical) with { Topics = new HashSet<Topic> { new("etn") } });
        var ids = ApprovingAll(redWithEthnicSense, Tractor, Box);

        // Act
        var objectAdjectives = ids.PoolsOf(IdKind.Object).Adjectives;
        var vehicleAdjectives = ids.PoolsOf(Vehicles).Adjectives;

        // Assert
        Assert.Empty(objectAdjectives);
        Assert.Empty(vehicleAdjectives);
    }

    [Fact]
    public void A_person_word_with_any_negative_sense_is_left_out()
    {
        // Arrange
        var tired = Adjective("træt", "trætte", Sense(MeaningCategory.Property, MeaningCategory.Mental) with { Sentiment = -1 });
        var ids = ApprovingAll(Happy, tired, Dancer);

        // Act
        var personAdjectives = ids.PoolsOf(IdKind.Person).Adjectives;

        // Assert
        Assert.Equal(["glade"], personAdjectives);
    }

    [Fact]
    public void An_object_adjective_needs_a_fitting_sense_that_is_not_negative()
    {
        // Arrange
        var cold = Adjective("kold", "kolde",
            Sense(MeaningCategory.Property, MeaningCategory.Physical),
            Sense(MeaningCategory.Property, MeaningCategory.Mental) with { Sentiment = -2 });
        var smelly = Adjective("ildelugtende", "ildelugtende",
            Sense(MeaningCategory.Property, MeaningCategory.Physical) with { Sentiment = -2 });
        var ids = ApprovingAll(cold, smelly, Box);

        // Act
        var objectAdjectives = ids.PoolsOf(IdKind.Object).Adjectives;

        // Assert
        Assert.Equal(["kolde"], objectAdjectives);
    }
}
