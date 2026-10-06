using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// What an identifier names, and so which adjectives and nouns it may be made of.
/// <see cref="MinimumSentiment"/> and <see cref="ExcludedTopics"/> apply to every sense of a word,
/// not just the sense that matched: a word with one negative or sensitive sense stays out.
/// The sense that matched must also meet its <see cref="SenseFilter.MinimumSentiment"/>.
/// </summary>
public sealed record IdKind(string Name, SenseFilter Adjectives, SenseFilter Nouns)
{
    public IdKind(string name, IReadOnlyCollection<MeaningCategory> adjectiveCategories, IReadOnlyCollection<MeaningCategory> nounCategories)
        : this(name, new SenseFilter(adjectiveCategories, []), new SenseFilter(nounCategories, []))
    {
    }

    /// <summary>COR.SEM centrality runs from 0 (peripheral) to 3 (core vocabulary).</summary>
    public int MinimumCentrality { get; init; } = 1;

    /// <summary>The lowest sentiment any sense of the word may have; an unrated sense counts as 0.</summary>
    public int MinimumSentiment { get; init; } = 0;

    public IReadOnlyCollection<Topic> ExcludedTopics { get; init; } = [];

    // A name given to a person must not hint at where they come from, their faith, politics or health.
    public static readonly IdKind Person = new(
        "Person",
        SenseFilter.Any(MeaningCategory.Mental),
        SenseFilter.Any(MeaningCategory.Human).Except(MeaningCategory.Group, MeaningCategory.Institution))
    {
        ExcludedTopics = [new Topic("geg"), new Topic("rel"), new Topic("pol"), new Topic("med")]
    };

    // A thing may have a negative sense elsewhere (kold as a personality) as long as the sense it is used in is not.
    // A noun that can also name a person (bager: the baker and the bakery) stays out, or a colour would read
    // as the person's skin.
    public static readonly IdKind Case = new(
        "Case",
        SenseFilter.Any(MeaningCategory.Physical, MeaningCategory.Colour).Except(MeaningCategory.Condition),
        SenseFilter.Any(
                MeaningCategory.Container, MeaningCategory.Furniture, MeaningCategory.Instrument, MeaningCategory.Vehicle,
                MeaningCategory.Garment, MeaningCategory.Building, MeaningCategory.Comestible)
            .ExceptWordsThatCanBe(MeaningCategory.Human))
    {
        MinimumSentiment = -3
    };
}
