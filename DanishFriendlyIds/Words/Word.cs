namespace DanishFriendlyIds.Words;

/// <summary>
/// One headword in one word class. A COR headword filed under two classes (an abbreviation next to
/// the full word, as with <c>ifølge</c>) is two words sharing an <see cref="Id"/>.
/// <see cref="Senses"/> is empty for words outside COR.SEM, which covers the ~34,000 most common headwords.
/// The summaries across senses are: categories and topics the union, sentiment the lowest, centrality the highest.
/// </summary>
public sealed record Word(
    WordId Id,
    string Lemma,
    WordClass WordClass,
    string? DefiniteForm,
    IReadOnlyList<WordSense> Senses,
    IReadOnlySet<Restriction> Restrictions)
{
    public bool HasMeaning => Senses.Count > 0;

    public IReadOnlySet<MeaningCategory> Categories => Senses.SelectMany(sense => sense.Categories).ToHashSet();

    public IReadOnlySet<Topic> Topics => Senses.SelectMany(sense => sense.Topics).ToHashSet();

    public int? MinimumSentiment => Senses.Min(sense => sense.Sentiment);

    public int? Centrality => Senses.Max(sense => sense.Centrality);

    public bool Is(MeaningCategory category) => Senses.Any(sense => sense.Is(category));
}
