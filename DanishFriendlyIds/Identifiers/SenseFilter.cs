using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// Matches a word when one of its senses has a category from <see cref="AnyOf"/> and none from
/// <see cref="NoneOf"/>. Matching per sense lets <c>rød</c> count as a colour although another of
/// its senses is "red in the face", and <c>bager</c> as a person although another sense is the bakery.
/// </summary>
public sealed record SenseFilter(IReadOnlyCollection<MeaningCategory> AnyOf, IReadOnlyCollection<MeaningCategory> NoneOf)
{
    public static SenseFilter Any(params MeaningCategory[] categories) => new(categories, []);

    public SenseFilter Except(params MeaningCategory[] categories) => this with { NoneOf = [.. NoneOf, .. categories] };

    public bool Matches(WordSense sense) => AnyOf.Any(sense.Is) && NoneOf.Any(sense.Is) == false;

    public bool MatchesASenseOf(Word word) => word.Senses.Any(Matches);
}
