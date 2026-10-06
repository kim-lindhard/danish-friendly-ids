using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// What an identifier names, and so which adjectives and nouns it may be made of.
/// <see cref="MinimumSentiment"/> and <see cref="ExcludedTopics"/> apply to every sense of a word,
/// not just the sense that matched: a word with one derogatory or sensitive sense stays out.
/// </summary>
public sealed record IdKind(string Name, SenseFilter Adjectives, SenseFilter Nouns)
{
    public IdKind(string name, IReadOnlyCollection<MeaningCategory> adjectiveCategories, IReadOnlyCollection<MeaningCategory> nounCategories)
        : this(name, new SenseFilter(adjectiveCategories, []), new SenseFilter(nounCategories, []))
    {
    }

    /// <summary>COR.SEM centrality runs from 0 (peripheral) to 3 (core vocabulary).</summary>
    public int MinimumCentrality { get; init; } = 1;

    /// <summary>COR.SEM sentiment runs from -3 to 3; a word with no sentiment counts as 0.</summary>
    public int MinimumSentiment { get; init; } = -1;

    public IReadOnlyCollection<Topic> ExcludedTopics { get; init; } = [];

    // A name given to a person must not hint at where they come from, their faith, politics, health or ethnicity.
    public static readonly IdKind Person = new(
        "Person",
        SenseFilter.Any(MeaningCategory.Mental),
        SenseFilter.Any(MeaningCategory.Human).Except(MeaningCategory.Group, MeaningCategory.Institution))
    {
        ExcludedTopics = [new Topic("geg"), new Topic("rel"), new Topic("pol"), new Topic("med"), new Topic("etn")]
    };

    public static readonly IdKind Case = new(
        "Case",
        SenseFilter.Any(MeaningCategory.Physical, MeaningCategory.Colour).Except(MeaningCategory.Condition),
        SenseFilter.Any(
                MeaningCategory.Container, MeaningCategory.Furniture, MeaningCategory.Instrument, MeaningCategory.Vehicle,
                MeaningCategory.Garment, MeaningCategory.Building, MeaningCategory.Comestible)
            .Except(MeaningCategory.Human))
    {
        MinimumSentiment = -3
    };
}
