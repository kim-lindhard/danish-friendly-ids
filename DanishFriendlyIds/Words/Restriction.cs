using System.Diagnostics.CodeAnalysis;

namespace DanishFriendlyIds.Words;

/// <summary>
/// Why a word may be unsuitable for general use. Usage and Rare come from COR.SEM's
/// <c>restriktion</c> column and are set on a word only when every one of its senses carries them,
/// so one slang sense of nine (as with <c>kasse</c>) does not mark the whole word.
/// Trademark comes from COR.EXT's registered-trademark flag.
/// </summary>
public sealed record Restriction(string Code, string EnglishName)
{
    public static readonly Restriction Usage = new("sprogbrug", "marked usage, e.g. slang, derogatory or old-fashioned");
    public static readonly Restriction Rare = new("frekvens", "rare");
    public static readonly Restriction Trademark = new("varemærke", "registered trademark");

    public static IReadOnlyList<Restriction> All { get; } = [Usage, Rare, Trademark];

    public static bool TryFromCode(string code, [NotNullWhen(true)] out Restriction? restriction)
    {
        restriction = All.FirstOrDefault(candidate => candidate.Code == code);
        return restriction is not null;
    }

    public override string ToString() => Code;
}
