using DanishFriendlyIds.Words;
using Xunit;
using static DanishFriendlyIds.TestSupport.TestWords;

namespace DanishFriendlyIds.Identifiers;

public class FriendlyIdGeneratorFacts
{
    [Fact]
    public void Every_drawn_person_identifier_comes_from_the_person_pools()
    {
        // Arrange
        var ids = new FriendlyIdGenerator(new Random(1));
        var pools = ids.PoolsOf(IdKind.Person);

        // Act
        var drawn = Enumerable.Range(0, 10_000).Select(_ => ids.Next(IdKind.Person)).ToList();

        // Assert
        Assert.All(drawn, id => Assert.Contains(id.Words[0], pools.FirstWords));
        Assert.All(drawn, id => Assert.Contains(id.Noun, pools.Nouns));
    }

    [Fact]
    public void The_same_seed_draws_the_same_identifiers()
    {
        // Arrange
        var first = new FriendlyIdGenerator(new Random(42));
        var second = new FriendlyIdGenerator(new Random(42));

        // Act
        var firstDraws = Enumerable.Range(0, 100).Select(_ => first.Next(IdKind.Object)).ToList();
        var secondDraws = Enumerable.Range(0, 100).Select(_ => second.Next(IdKind.Object)).ToList();

        // Assert
        Assert.Equal(firstDraws, secondDraws);
    }

    [Fact]
    public void A_word_without_a_verdict_is_never_drawn()
    {
        // Arrange
        var ids = PeopleListOnly([Approved(Happy), Approved(Dancer)], Happy, Calm, Dancer, Cyclist);

        // Act
        var drawn = Drawn(ids, IdKind.Person);

        // Assert
        Assert.Equal([FriendlyId.Of("glade", "danser")], drawn);
    }

    [Fact]
    public void A_rejected_word_is_never_drawn()
    {
        // Arrange
        var rejectedCyclist = new ReviewEntry("cyklist", ReviewSubject.Noun, ReviewVerdict.Rejected, "test");
        var ids = PeopleListOnly([Approved(Happy), Approved(Dancer), rejectedCyclist], Happy, Dancer, Cyclist);

        // Act
        var drawn = Drawn(ids, IdKind.Person);

        // Assert
        Assert.Equal([FriendlyId.Of("glade", "danser")], drawn);
    }

    [Fact]
    public void A_blocked_pair_is_never_drawn_and_does_not_count_towards_capacity()
    {
        // Arrange
        var blocked = new ReviewEntry("glade danser", ReviewSubject.Pair, ReviewVerdict.Rejected, "test");
        var ids = PeopleListOnly([Approved(Happy), Approved(Calm), Approved(Dancer), blocked], Happy, Calm, Dancer);

        // Act
        var drawn = Drawn(ids, IdKind.Person);
        var capacity = ids.CapacityOf(IdKind.Person);

        // Assert
        Assert.Equal([FriendlyId.Of("rolige", "danser")], drawn);
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
        Assert.Equal(FriendlyId.Of("glade", "cyklist"), free);
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
        Assert.Equal([FriendlyId.Of("røde", "traktor")], drawn);
    }

    [Fact]
    public void A_kind_whose_candidates_are_unreviewed_cannot_draw_and_says_why()
    {
        // Arrange
        var ids = PeopleListOnly([Approved(Happy), Approved(Dancer)], Happy, Red, Dancer, Tractor);

        // Act
        var error = Assert.Throws<InvalidOperationException>(() => ids.Next(Vehicles));
        var found = ids.TryNext(Vehicles, _ => false, out var free);

        // Assert
        Assert.Contains("1 adjectives, 0 -ende words and 1 nouns among its candidates are unreviewed", error.Message);
        Assert.False(found);
        Assert.Null(free);
    }
}
