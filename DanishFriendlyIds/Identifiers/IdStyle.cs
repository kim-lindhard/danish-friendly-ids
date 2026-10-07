using System.Diagnostics.CodeAnalysis;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// How an identifier is written. <see cref="Ascii"/> and <see cref="UrlSlug"/> fold æ→ae, ø→oe, å→aa,
/// the same mapping as Enablers' IdStringRules; a URL slug is lowercase a–z, digits and hyphens only,
/// all of them unreserved in a URL. No two approved words fold to the same spelling, so every style is
/// as unique as the Danish one.
/// </summary>
public sealed record IdStyle(string Name, bool IsFolded, char Separator)
{
    public static readonly IdStyle Danish = new("danish", false, ' ');
    public static readonly IdStyle Ascii = new("ascii", true, ' ');
    public static readonly IdStyle UrlSlug = new("url", true, '-');

    public static IReadOnlyList<IdStyle> All { get; } = [Danish, Ascii, UrlSlug];

    public string Write(string word) => IsFolded ? Fold(word) : word;

    public static string Fold(string word) => word
        .Replace("æ", "ae", StringComparison.Ordinal)
        .Replace("ø", "oe", StringComparison.Ordinal)
        .Replace("å", "aa", StringComparison.Ordinal);

    public static bool TryFromName(string name, [NotNullWhen(true)] out IdStyle? style)
    {
        style = All.FirstOrDefault(candidate => candidate.Name == name);
        return style is not null;
    }

    public override string ToString() => Name;
}
