using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// What an identifier names, so which adjectives and nouns it may be made of, and which reviewed
/// <see cref="Vocabulary"/> the words must be approved in.
/// <see cref="MinimumSentiment"/> and <see cref="ExcludedTopics"/> apply to every sense of a word,
/// not just the sense that matched: a word with one negative or sensitive sense stays out.
/// The sense that matched must also meet its <see cref="SenseFilter.MinimumSentiment"/>.
/// </summary>
public sealed record IdKind(string Name, Vocabulary Vocabulary, SenseFilter Adjectives, SenseFilter Nouns)
{
    public IdKind(
        string name,
        Vocabulary vocabulary,
        IReadOnlyCollection<MeaningCategory> adjectiveCategories,
        IReadOnlyCollection<MeaningCategory> nounCategories)
        : this(name, vocabulary, new SenseFilter(adjectiveCategories, []), new SenseFilter(nounCategories, []))
    {
    }

    /// <summary>
    /// COR.SEM centrality runs from 0 (peripheral) to 3 (core vocabulary). The default, 1, keeps a kind to
    /// common words; <see cref="WithLessCommonWords"/> also lets in the peripheral ones the review approved.
    /// </summary>
    public int MinimumCentrality { get; init; } = 1;

    public bool UsesLessCommonWords => MinimumCentrality == 0;

    /// <summary>The same kind, also drawing less common words (centrality 0) that its review list approved.</summary>
    public IdKind WithLessCommonWords() => this with { MinimumCentrality = 0 };

    /// <summary>The lowest sentiment any sense of the word may have; an unrated sense counts as 0.</summary>
    public int MinimumSentiment { get; init; } = 0;

    public IReadOnlyCollection<Topic> ExcludedTopics { get; init; } = [];

    /// <summary>Which verbs may give an -ende word (dansende, blinkende); none unless a kind sets it.</summary>
    public SenseFilter Participles { get; init; } = SenseFilter.Nothing;

    // A name given to a person must not hint at where they come from, their faith, politics or health.
    // Physical traits count (energisk, adræt); the review keeps out the ones about bodies.
    public static readonly IdKind Person = new(
        "Person",
        Vocabulary.People,
        SenseFilter.Any(MeaningCategory.Mental, MeaningCategory.Physical).Except(MeaningCategory.Condition),
        SenseFilter.Any(MeaningCategory.Human).Except(MeaningCategory.Group, MeaningCategory.Institution))
    {
        ExcludedTopics = [new Topic("geg"), new Topic("rel"), new Topic("pol"), new Topic("med")],
        Participles = SenseFilter.Any(
                MeaningCategory.Act, MeaningCategory.Communication, MeaningCategory.Experience,
                MeaningCategory.Mental, MeaningCategory.Social)
            .Except(MeaningCategory.Condition)
    };

    // A thing may have a negative sense elsewhere (kold as a personality) as long as the sense it is used in is not.
    // A noun that can also name a person (bager: the baker and the bakery) stays out, or a colour would read
    // as the person's skin.
    public static readonly IdKind Object = new(
        "Object",
        Vocabulary.Objects,
        SenseFilter.Any(MeaningCategory.Physical, MeaningCategory.Colour).Except(MeaningCategory.Condition),
        SenseFilter.Any(
                MeaningCategory.Container, MeaningCategory.Furniture, MeaningCategory.Instrument, MeaningCategory.Vehicle,
                MeaningCategory.Garment, MeaningCategory.Building, MeaningCategory.Comestible)
            .ExceptWordsThatCanBe(MeaningCategory.Human))
    {
        MinimumSentiment = -3,
        Participles = SenseFilter.Any(MeaningCategory.Physical, MeaningCategory.Event, MeaningCategory.Existence)
            .Except(MeaningCategory.Condition)
    };

    public static IReadOnlyList<IdKind> Presets { get; } = [Person, Object];

    /// <summary>Every preset, with common words only and with less common words too.</summary>
    public static IReadOnlyList<IdKind> PresetsWithWordChoices { get; } =
        [Person, Person.WithLessCommonWords(), Object, Object.WithLessCommonWords()];
}
