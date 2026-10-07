using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// Matches a word when one of its senses has a category from <see cref="AnyOf"/>, none from
/// <see cref="NoneOf"/>, and a sentiment of at least <see cref="MinimumSentiment"/>. Matching per sense
/// lets <c>rød</c> count as a colour although another of its senses is "red in the face", and
/// <c>kold</c> as a temperature although it is negative as a personality.
/// </summary>
public sealed record SenseFilter(IReadOnlyCollection<MeaningCategory> AnyOf, IReadOnlyCollection<MeaningCategory> NoneOf)
{
    /// <summary>COR.SEM sentiment runs from -3 to 3; an unrated sense counts as 0.</summary>
    public int MinimumSentiment { get; init; } = 0;

    /// <summary>Categories no sense of the word may have, whichever sense matched.</summary>
    public IReadOnlyCollection<MeaningCategory> NoSenseOf { get; init; } = [];

    public static SenseFilter Nothing { get; } = new([], []);

    public static SenseFilter Any(params MeaningCategory[] categories) => new(categories, []);

    public SenseFilter Except(params MeaningCategory[] categories) => this with { NoneOf = [.. NoneOf, .. categories] };

    public SenseFilter ExceptWordsThatCanBe(params MeaningCategory[] categories) =>
        this with { NoSenseOf = [.. NoSenseOf, .. categories] };

    public bool Matches(WordSense sense)
    {
        var hasAWantedCategory = AnyOf.Any(sense.Is);
        var hasNoUnwantedCategory = NoneOf.Any(sense.Is) == false;
        var isPositiveEnough = MinimumSentiment <= (sense.Sentiment ?? 0);
        return hasAWantedCategory && hasNoUnwantedCategory && isPositiveEnough;
    }

    public bool MatchesASenseOf(Word word) => word.Senses.Any(Matches) && NoSenseOf.Any(word.Is) == false;
}
