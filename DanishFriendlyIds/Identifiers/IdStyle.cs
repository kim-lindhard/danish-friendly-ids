using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// How an identifier is written. <see cref="Ascii"/>, <see cref="UrlSlug"/> and <see cref="WithAsciiLetters"/>
/// fold æ→ae, ø→oe, å→aa, the same mapping as Enablers' IdStringRules; a URL slug is lowercase a–z, digits and
/// hyphens only, all of them unreserved in a URL. No two approved words fold to the same spelling, so every
/// style is as unique as the Danish one. Every word is lowercase, so in <see cref="PascalCase"/> and
/// <see cref="CamelCase"/> a capital can only mark where a word starts, and they read back unambiguously.
/// </summary>
public sealed record IdStyle
{
    public static readonly IdStyle Danish = new("danish", " ", null, false);
    public static readonly IdStyle Ascii = new("ascii", " ", null, true);
    public static readonly IdStyle UrlSlug = new("url", "-", null, true);
    public static readonly IdStyle PascalCase = new("pascal", "", 0, false);
    public static readonly IdStyle CamelCase = new("camel", "", 1, false);
    private static readonly IdStyle PascalCaseAscii = new("pascal-ascii", "", 0, true);
    private static readonly IdStyle CamelCaseAscii = new("camel-ascii", "", 1, true);

    public static IReadOnlyList<IdStyle> All { get; } =
        [Danish, Ascii, UrlSlug, PascalCase, PascalCaseAscii, CamelCase, CamelCaseAscii];

    private IdStyle(string name, string separator, int? firstCapitalisedWord, bool isFolded)
    {
        Name = name;
        Separator = separator;
        FirstCapitalisedWord = firstCapitalisedWord;
        IsFolded = isFolded;
    }

    public string Name { get; }

    public string Separator { get; }

    public bool IsFolded { get; }

    // 0 in PascalCase, 1 in camelCase: that word and every word after it start with a capital.
    private int? FirstCapitalisedWord { get; }

    /// <summary>The same style with æ, ø, å folded to ae, oe, aa: <c>Danish.WithAsciiLetters()</c> is <see cref="Ascii"/>.</summary>
    public IdStyle WithAsciiLetters() => All.Single(style =>
        style.IsFolded && style.Separator == Separator && style.FirstCapitalisedWord == FirstCapitalisedWord);

    internal string Write(IReadOnlyList<string> words, int? number)
    {
        var parts = words
            .Select((word, index) => Capitalised(IsFolded ? Fold(word) : word, index))
            .Concat(number is { } value ? [value.ToString(CultureInfo.InvariantCulture)] : []);
        return string.Join(Separator, parts);
    }

    private string Capitalised(string word, int index) =>
        FirstCapitalisedWord <= index ? char.ToUpperInvariant(word[0]) + word[1..] : word;

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
