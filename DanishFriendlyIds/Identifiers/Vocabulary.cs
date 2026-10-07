namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// Which reviewed word list a kind draws from. A word can fit one list and not the other:
/// energiske describes a person, elektriske a thing. Each list has its own review file.
/// </summary>
public sealed record Vocabulary(string Name, string ReviewResourceName)
{
    public static readonly Vocabulary People = new("people", "DanishFriendlyIds.review-people.tsv");
    public static readonly Vocabulary Objects = new("objects", "DanishFriendlyIds.review-objects.tsv");

    public static IReadOnlyList<Vocabulary> All { get; } = [People, Objects];

    public override string ToString() => Name;
}
