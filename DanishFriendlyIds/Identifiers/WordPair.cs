using System.Diagnostics.CodeAnalysis;

namespace DanishFriendlyIds.Identifiers;

/// <summary>Two words that may not appear together in an identifier, in this order: "tomme tønde".</summary>
public sealed record WordPair(string First, string Second)
{
    public static bool TryParse(string text, [NotNullWhen(true)] out WordPair? pair)
    {
        var words = text.Split(' ');
        var isTwoWords = words.Length == 2 && words.All(word => word.Length > 0);
        pair = isTwoWords ? new WordPair(words[0], words[1]) : null;
        return pair is not null;
    }

    public override string ToString() => $"{First} {Second}";
}
