using DanishFriendlyIds.Words;
using Xunit;
using static DanishFriendlyIds.TestSupport.TestWords;

namespace DanishFriendlyIds.Identifiers;

/// <summary>Two and three words, the number suffix, and exact capacity per format.</summary>
public class FormatFacts
{
    private static readonly Word Exciting = Adjective("spændende", "spændende", Sense(MeaningCategory.Property, MeaningCategory.Mental));
    private static readonly Word Thrilling = Verb("spænde", "spændende", Sense(MeaningCategory.Act, MeaningCategory.Mental));

    [Fact]
    public void Two_words_start_with_an_adjective_or_an_ende_word()
    {
        // Arrange
        var ids = ApprovingAll(Happy, Dancing, Pilot);

        // Act
        var drawn = Drawn(ids, IdKind.Person);

        // Assert
        Assert.Equal(
            new HashSet<FriendlyId> { FriendlyId.Of("glade", "pilot"), FriendlyId.Of("dansende", "pilot") },
            drawn.ToHashSet());
    }

    [Fact]
    public void Three_words_are_an_adjective_an_ende_word_and_a_noun()
    {
        // Arrange
        var ids = ApprovingAll(Happy, Calm, Dancing, Singing, Pilot);

        // Act
        var drawn = Drawn(ids, IdKind.Person, IdFormat.ThreeWords);

        // Assert
        Assert.All(drawn, id => Assert.Equal(3, id.Words.Count));
        Assert.All(drawn, id => Assert.Contains(id.Words[0], new[] { "glade", "rolige" }));
        Assert.All(drawn, id => Assert.Contains(id.Words[1], new[] { "dansende", "syngende" }));
        Assert.All(drawn, id => Assert.Equal("pilot", id.Noun));
        Assert.Equal(4, drawn.Count);
    }

    [Fact]
    public void A_word_that_is_both_adjective_and_ende_word_is_never_used_twice()
    {
        // Arrange
        var ids = ApprovingAll(Exciting, Happy, Thrilling, Dancing, Pilot);

        // Act
        var drawn = Drawn(ids, IdKind.Person, IdFormat.ThreeWords);
        var capacity = ids.CapacityOf(IdKind.Person, IdFormat.ThreeWords);

        // Assert
        Assert.DoesNotContain(FriendlyId.Of("spændende", "spændende", "pilot"), drawn);
        Assert.Equal(3, drawn.Count);
        Assert.Equal(3, capacity);
    }

    [Fact]
    public void A_number_suffix_runs_from_one_to_the_maximum()
    {
        // Arrange
        var ids = ApprovingAll(Happy, Pilot);
        var format = IdFormat.TwoWords.WithNumber(9);

        // Act
        var numbers = Drawn(ids, IdKind.Person, format).Select(id => id.Number).ToHashSet();
        var capacity = ids.CapacityOf(IdKind.Person, format);

        // Assert
        Assert.Equal(Enumerable.Range(1, 9).Select(number => (int?)number).ToHashSet(), numbers);
        Assert.Equal(9, capacity);
    }

    [Fact]
    public void A_number_suffix_needs_a_maximum_of_at_least_two()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IdFormat.TwoWords.WithNumber(1));
    }

    [Fact]
    public void Capacity_subtracts_every_combination_that_holds_a_blocked_pair()
    {
        // Arrange
        var words = new[] { Happy, Calm, Dancing, Singing, Pilot, Baker };
        var ids = PeopleListOnly(
            words.Select(Approved).Concat([BlockedPair("glade", "pilot"), BlockedPair("syngende", "bager")]),
            words);

        // Act
        var twoWords = ids.CapacityOf(IdKind.Person, IdFormat.TwoWords);
        var threeWords = ids.CapacityOf(IdKind.Person, IdFormat.ThreeWords);
        var threeWordsWithNumbers = ids.CapacityOf(IdKind.Person, IdFormat.ThreeWords.WithNumber(99));

        // Assert
        Assert.Equal(4 * 2 - 2, twoWords);
        Assert.Equal(2 * 2 * 2 - 2 - 2, threeWords);
        Assert.Equal(threeWords * 99, threeWordsWithNumbers);
    }

    [Fact]
    public void A_blocked_pair_is_never_drawn_inside_three_words()
    {
        // Arrange
        var words = new[] { Happy, Calm, Dancing, Singing, Pilot, Baker };
        var ids = PeopleListOnly(words.Select(Approved).Append(BlockedPair("glade", "pilot")), words);

        // Act
        var drawn = Drawn(ids, IdKind.Person, IdFormat.ThreeWords);

        // Assert
        Assert.DoesNotContain(drawn, id => id.Words[0] == "glade" && id.Noun == "pilot");
        Assert.Equal(6, drawn.Count);
    }

    [Fact]
    public void A_kind_without_ende_words_cannot_make_three_words()
    {
        // Arrange
        var ids = ApprovingAll(Red, Dancing, Tractor);

        // Act
        var twoWords = ids.Next(Vehicles);
        var found = ids.TryNext(Vehicles, IdFormat.ThreeWords, _ => false, out var threeWords);

        // Assert
        Assert.Equal(FriendlyId.Of("røde", "traktor"), twoWords);
        Assert.False(found);
        Assert.Null(threeWords);
        Assert.Throws<InvalidOperationException>(() => ids.Next(Vehicles, IdFormat.ThreeWords));
    }

    [Fact]
    public void An_ende_word_approved_only_for_objects_is_never_drawn_for_a_person()
    {
        // Arrange
        var ids = Generator(
            peopleList: [Approved(Happy), Approved(Pilot)],
            objectsList: [Approved(Dancing)],
            Happy, Dancing, Pilot);

        // Act
        var participles = ids.PoolsOf(IdKind.Person).Participles;

        // Assert
        Assert.Empty(participles);
    }
}
