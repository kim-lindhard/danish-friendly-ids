namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// The words one kind of identifier is drawn from, each list distinct and in ordinal order. Participles
/// are the verbs' -ende forms; in two-word identifiers they share the first slot with the adjectives.
/// </summary>
public sealed record WordPools(IReadOnlyList<string> Adjectives, IReadOnlyList<string> Participles, IReadOnlyList<string> Nouns)
{
    public IReadOnlyList<string> FirstWords { get; } =
        Adjectives.Concat(Participles).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();

    public bool CanMake(IdFormat format) =>
        format.WordCount == 2
            ? FirstWords.Count > 0 && Nouns.Count > 0
            : Adjectives.Count > 0 && Participles.Count > 0 && Nouns.Count > 0;
}
