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
    private static readonly Word Calm = Adjective("rolig", "rolige", Sense(MeaningCategory.Property, MeaningCategory.Mental));
    private static readonly Word Red = Adjective("rød", "røde",
        Sense(MeaningCategory.Property, MeaningCategory.Physical, MeaningCategory.Condition),
        Sense(MeaningCategory.Property, MeaningCategory.Colour));
    private static readonly Word Flushed = Adjective("rødmosset", "rødmossede",
        Sense(MeaningCategory.Property, MeaningCategory.Physical, MeaningCategory.Condition));
    private static readonly Word Dancer = Noun("danser", Sense(MeaningCategory.Human, MeaningCategory.Object));
    private static readonly Word Cyclist = Noun("cyklist", Sense(MeaningCategory.Human, MeaningCategory.Object));
    private static readonly Word Tractor = Noun("traktor", Sense(MeaningCategory.Vehicle, MeaningCategory.Artifact, MeaningCategory.Object));
    private static readonly Word Box = Noun("kasse", Sense(MeaningCategory.Container, MeaningCategory.Artifact, MeaningCategory.Object));

    private static readonly IdKind Vehicles = new("Køretøj", [MeaningCategory.Colour], [MeaningCategory.Vehicle]);

    private static ReviewEntry Approved(Word word) => new(
        word.DefiniteForm ?? word.Lemma,
        word.WordClass == WordClass.Adjective ? ReviewSubject.Adjective : ReviewSubject.Noun,
        ReviewVerdict.Approved,
        "");

    private static FriendlyIdGenerator Generator(IEnumerable<ReviewEntry> review, params Word[] words) =>
        new(new Lexicon(words), new WordReview(review), new Random(7));

    private static FriendlyIdGenerator ApprovingAll(params Word[] words) => Generator(words.Select(Approved), words);

    private static List<FriendlyId> Drawn(FriendlyIdGenerator ids, IdKind kind) =>
        Enumerable.Range(0, 200).Select(_ => ids.Next(kind)).Distinct().ToList();

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
    public void A_word_without_a_verdict_is_never_drawn()
    {
        // Arrange
        var ids = Generator([Approved(Happy), Approved(Dancer)], Happy, Calm, Dancer, Cyclist);

        // Act
        var drawn = Drawn(ids, IdKind.Person);

        // Assert
        Assert.Equal([new FriendlyId("glade", "danser")], drawn);
    }

    [Fact]
    public void A_rejected_word_is_never_drawn()
    {
        // Arrange
        var rejectedCyclist = new ReviewEntry("cyklist", ReviewSubject.Noun, ReviewVerdict.Rejected, "test");
        var ids = Generator([Approved(Happy), Approved(Dancer), rejectedCyclist], Happy, Dancer, Cyclist);

        // Act
        var drawn = Drawn(ids, IdKind.Person);

        // Assert
        Assert.Equal([new FriendlyId("glade", "danser")], drawn);
    }

    [Fact]
    public void A_blocked_pair_is_never_drawn_and_does_not_count_towards_capacity()
    {
        // Arrange
        var blocked = new ReviewEntry("glade danser", ReviewSubject.Pair, ReviewVerdict.Rejected, "test");
        var ids = Generator([Approved(Happy), Approved(Calm), Approved(Dancer), blocked], Happy, Calm, Dancer);

        // Act
        var drawn = Drawn(ids, IdKind.Person);
        var capacity = ids.CapacityOf(IdKind.Person);

        // Assert
        Assert.Equal([new FriendlyId("rolige", "danser")], drawn);
        Assert.Equal(1, capacity);
    }

    [Fact]
    public void Capacity_is_adjectives_times_nouns()
    {
        // Arrange
        var ids = ApprovingAll(Happy, Calm, Dancer, Cyclist);

        // Act
        var capacity = ids.CapacityOf(IdKind.Person);

        // Assert
        Assert.Equal(4, capacity);
    }

    [Fact]
    public void TryNext_skips_identifiers_that_are_taken()
    {
        // Arrange
        var ids = ApprovingAll(Happy, Dancer, Cyclist);

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
        var ids = ApprovingAll(Happy, Dancer, Cyclist);

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
        var ids = ApprovingAll(Happy, Red, Dancer, Tractor);

        // Act
        var drawn = Drawn(ids, Vehicles);

        // Assert
        Assert.Equal([new FriendlyId("røde", "traktor")], drawn);
    }

    [Fact]
    public void A_word_qualifies_through_any_one_of_its_senses()
    {
        // Arrange
        var ids = ApprovingAll(Red, Flushed, Tractor);

        // Act
        var caseAdjectives = ids.PoolsOf(IdKind.Case).Adjectives;

        // Assert
        Assert.Equal(["røde"], caseAdjectives);
    }

    [Fact]
    public void A_kind_whose_candidates_are_unreviewed_cannot_draw_and_says_why()
    {
        // Arrange
        var ids = Generator([Approved(Happy), Approved(Dancer)], Happy, Red, Dancer, Tractor);

        // Act
        var error = Assert.Throws<InvalidOperationException>(() => ids.Next(Vehicles));
        var found = ids.TryNext(Vehicles, _ => false, out var free);

        // Assert
        Assert.Contains("1 adjectives and 1 nouns among its candidates are unreviewed", error.Message);
        Assert.False(found);
        Assert.Null(free);
    }

    [Fact]
    public void A_noun_that_can_name_a_person_is_never_a_case_noun()
    {
        // Arrange
        var baker = Noun("bager",
            Sense(MeaningCategory.Human, MeaningCategory.Object, MeaningCategory.Occupation),
            Sense(MeaningCategory.Building, MeaningCategory.Artifact, MeaningCategory.Object));
        var ids = ApprovingAll(Happy, Red, baker, Box);

        // Act
        var caseNouns = ids.PoolsOf(IdKind.Case).Nouns;
        var personNouns = ids.PoolsOf(IdKind.Person).Nouns;

        // Assert
        Assert.Equal(["kasse"], caseNouns);
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
        var caseAdjectives = ids.PoolsOf(IdKind.Case).Adjectives;
        var vehicleAdjectives = ids.PoolsOf(Vehicles).Adjectives;

        // Assert
        Assert.Empty(caseAdjectives);
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
    public void A_case_adjective_needs_a_fitting_sense_that_is_not_negative()
    {
        // Arrange
        var cold = Adjective("kold", "kolde",
            Sense(MeaningCategory.Property, MeaningCategory.Physical),
            Sense(MeaningCategory.Property, MeaningCategory.Mental) with { Sentiment = -2 });
        var smelly = Adjective("ildelugtende", "ildelugtende",
            Sense(MeaningCategory.Property, MeaningCategory.Physical) with { Sentiment = -2 });
        var ids = ApprovingAll(cold, smelly, Box);

        // Act
        var caseAdjectives = ids.PoolsOf(IdKind.Case).Adjectives;

        // Assert
        Assert.Equal(["kolde"], caseAdjectives);
    }
}
