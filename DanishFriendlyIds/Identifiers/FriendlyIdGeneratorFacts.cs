using DanishFriendlyIds.Words;
using Xunit;

namespace DanishFriendlyIds.Identifiers;

public class FriendlyIdGeneratorFacts
{
    private static WordSense Sense(params MeaningCategory[] categories) =>
        new(categories.ToHashSet(), new HashSet<Topic>(), null, 3, new HashSet<Restriction>());

    private static Word Adjective(string lemma, string definiteForm, params WordSense[] senses) =>
        new(new WordId($"TEST.{lemma}"), lemma, WordClass.Adjective, definiteForm, senses, new HashSet<Restriction>());

    private static Word Noun(string lemma, params WordSense[] senses) =>
        new(new WordId($"TEST.{lemma}"), lemma, WordClass.Noun, null, senses, new HashSet<Restriction>());

    private static readonly Word Happy = Adjective("glad", "glade", Sense(MeaningCategory.Property, MeaningCategory.Mental));
    private static readonly Word Tired = Adjective("træt", "trætte", Sense(MeaningCategory.Property, MeaningCategory.Mental));
    private static readonly Word Red = Adjective("rød", "røde",
        Sense(MeaningCategory.Property, MeaningCategory.Physical, MeaningCategory.Condition),
        Sense(MeaningCategory.Property, MeaningCategory.Colour));
    private static readonly Word Flushed = Adjective("rødmosset", "rødmossede",
        Sense(MeaningCategory.Property, MeaningCategory.Physical, MeaningCategory.Condition));
    private static readonly Word Dancer = Noun("danser", Sense(MeaningCategory.Human, MeaningCategory.Object));
    private static readonly Word Cyclist = Noun("cyklist", Sense(MeaningCategory.Human, MeaningCategory.Object));
    private static readonly Word Tractor = Noun("traktor", Sense(MeaningCategory.Vehicle, MeaningCategory.Artifact, MeaningCategory.Object));

    private static FriendlyIdGenerator Generator(Blocklist? blocklist = null, params Word[] words) =>
        new(new Lexicon(words), blocklist ?? Blocklist.Empty, new Random(7));

    private static readonly IdKind Vehicles = new("Køretøj", [MeaningCategory.Colour], [MeaningCategory.Vehicle]);

    [Fact]
    public void Every_drawn_person_identifier_comes_from_the_person_pools()
    {
        // Arrange
        var ids = new FriendlyIdGenerator(new Random(1));
        var pools = ids.PoolsOf(IdKind.Person);

        // Act
        var drawn = Enumerable.Range(0, 10_000).Select(_ => ids.Next(IdKind.Person)).ToList();

        // Assert
        Assert.All(drawn, id => Assert.Contains(id.Adjective, pools.Adjectives));
        Assert.All(drawn, id => Assert.Contains(id.Noun, pools.Nouns));
    }

    [Fact]
    public void The_same_seed_draws_the_same_identifiers()
    {
        // Arrange
        var first = new FriendlyIdGenerator(new Random(42));
        var second = new FriendlyIdGenerator(new Random(42));

        // Act
        var firstDraws = Enumerable.Range(0, 100).Select(_ => first.Next(IdKind.Case)).ToList();
        var secondDraws = Enumerable.Range(0, 100).Select(_ => second.Next(IdKind.Case)).ToList();

        // Assert
        Assert.Equal(firstDraws, secondDraws);
    }

    [Fact]
    public void A_blocked_lemma_or_definite_form_is_never_drawn()
    {
        // Arrange
        var ids = Generator(new Blocklist(["danser", "trætte"]), Happy, Tired, Dancer, Cyclist);

        // Act
        var drawn = Enumerable.Range(0, 200).Select(_ => ids.Next(IdKind.Person)).Distinct().ToList();

        // Assert
        Assert.Equal([new FriendlyId("glade", "cyklist")], drawn);
    }

    [Fact]
    public void Capacity_is_adjectives_times_nouns()
    {
        // Arrange
        var ids = Generator(null, Happy, Tired, Dancer, Cyclist);

        // Act
        var capacity = ids.CapacityOf(IdKind.Person);

        // Assert
        Assert.Equal(4, capacity);
    }

    [Fact]
    public void TryNext_skips_identifiers_that_are_taken()
    {
        // Arrange
        var ids = Generator(null, Happy, Dancer, Cyclist);

        // Act
        var found = ids.TryNext(IdKind.Person, id => id.Noun == "danser", out var free);

        // Assert
        Assert.True(found);
        Assert.Equal(new FriendlyId("glade", "cyklist"), free);
    }

    [Fact]
    public void TryNext_gives_up_when_every_identifier_is_taken()
    {
        // Arrange
        var ids = Generator(null, Happy, Dancer, Cyclist);

        // Act
        var found = ids.TryNext(IdKind.Person, _ => true, out var free);

        // Assert
        Assert.False(found);
        Assert.Null(free);
    }

    [Fact]
    public void A_project_can_define_its_own_kind_from_categories()
    {
        // Arrange
        var ids = Generator(null, Happy, Red, Dancer, Tractor);

        // Act
        var drawn = Enumerable.Range(0, 50).Select(_ => ids.Next(Vehicles)).Distinct().ToList();

        // Assert
        Assert.Equal([new FriendlyId("røde", "traktor")], drawn);
    }

    [Fact]
    public void A_word_qualifies_through_any_one_of_its_senses()
    {
        // Arrange
        var ids = Generator(null, Red, Flushed, Tractor);

        // Act
        var caseAdjectives = ids.PoolsOf(IdKind.Case).Adjectives;

        // Assert
        Assert.Equal(["røde"], caseAdjectives);
    }

    [Fact]
    public void A_kind_with_an_empty_pool_cannot_draw()
    {
        // Arrange
        var ids = Generator(null, Happy, Dancer);

        // Act
        var found = ids.TryNext(Vehicles, _ => false, out var free);

        // Assert
        Assert.Throws<InvalidOperationException>(() => ids.Next(Vehicles));
        Assert.False(found);
        Assert.Null(free);
    }

    [Fact]
    public void A_person_word_with_one_sensitive_sense_is_left_out()
    {
        // Arrange
        var dancerWithEthnicSense = Noun("danser",
            Sense(MeaningCategory.Human, MeaningCategory.Object),
            Sense(MeaningCategory.Human, MeaningCategory.Group) with { Topics = new HashSet<Topic> { new("etn") } });
        var ids = Generator(null, Happy, dancerWithEthnicSense, Cyclist);

        // Act
        var nouns = ids.PoolsOf(IdKind.Person).Nouns;

        // Assert
        Assert.Equal(["cyklist"], nouns);
    }

    [Fact]
    public void A_person_adjective_with_a_clearly_negative_sense_is_left_out_but_a_case_may_have_it()
    {
        // Arrange
        var cold = Adjective("kold", "kolde",
            Sense(MeaningCategory.Property, MeaningCategory.Physical),
            Sense(MeaningCategory.Property, MeaningCategory.Mental) with { Sentiment = -2 });
        var box = Noun("kasse", Sense(MeaningCategory.Container, MeaningCategory.Artifact, MeaningCategory.Object));
        var ids = Generator(null, Happy, cold, Dancer, box);

        // Act
        var personAdjectives = ids.PoolsOf(IdKind.Person).Adjectives;
        var caseAdjectives = ids.PoolsOf(IdKind.Case).Adjectives;

        // Assert
        Assert.Equal(["glade"], personAdjectives);
        Assert.Equal(["kolde"], caseAdjectives);
    }
}
