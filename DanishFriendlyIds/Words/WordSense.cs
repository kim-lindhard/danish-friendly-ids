namespace DanishFriendlyIds.Words;

/// <summary>One COR.SEM sense of a word: what the word means in one of its uses.</summary>
public sealed record WordSense(
    IReadOnlySet<MeaningCategory> Categories,
    IReadOnlySet<Topic> Topics,
    int? Sentiment,
    int? Centrality,
    IReadOnlySet<Restriction> Restrictions)
{
    public bool Is(MeaningCategory category) => Categories.Contains(category);
}
