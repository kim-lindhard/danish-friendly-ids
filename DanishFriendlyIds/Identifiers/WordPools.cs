namespace DanishFriendlyIds.Identifiers;

/// <summary>The words one kind of identifier is drawn from, each list distinct and in ordinal order.</summary>
public sealed record WordPools(IReadOnlyList<string> Adjectives, IReadOnlyList<string> Nouns)
{
    public long Capacity => (long)Adjectives.Count * Nouns.Count;

    public bool IsEmpty => Adjectives.Count == 0 || Nouns.Count == 0;
}
