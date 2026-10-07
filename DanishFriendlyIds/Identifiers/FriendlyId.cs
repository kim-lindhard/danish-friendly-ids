using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// Two or three words, the last a noun, optionally followed by a number: "glade dansende pilot 42".
/// <see cref="ToString(IdStyle)"/> writes it in Danish, in ASCII, as a URL slug, or in PascalCase or camelCase.
/// </summary>
public sealed partial record FriendlyId
{
    private FriendlyId(IReadOnlyList<string> words, int? number)
    {
        Words = words;
        Number = number;
    }

    public IReadOnlyList<string> Words { get; }

    public int? Number { get; }

    public string Noun => Words[^1];

    public static FriendlyId Of(params string[] words)
    {
        var hasTwoOrThreeWords = 2 <= words.Length && words.Length <= 3;
        var everyWordIsLetters = words.All(word => word.Length > 0 && word.All(char.IsLetter));
        return hasTwoOrThreeWords && everyWordIsLetters
            ? new FriendlyId(words.ToArray(), null)
            : throw new ArgumentException("An identifier is two or three words of letters", nameof(words));
    }

    public FriendlyId WithNumber(int number) => 1 <= number
        ? new FriendlyId(Words, number)
        : throw new ArgumentOutOfRangeException(nameof(number), number, "The number must be at least 1");

    public bool Equals(FriendlyId? other) =>
        other is not null && Words.SequenceEqual(other.Words, StringComparer.Ordinal) && Number == other.Number;

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(ToString());

    public override string ToString() => ToString(IdStyle.Danish);

    public string ToString(IdStyle style) => style.Write(Words, Number);

    /// <summary>
    /// Reads any style: words separated by spaces or by hyphens (not both), or joined in PascalCase or camelCase,
    /// then an optional number.
    /// </summary>
    public static bool TryParse(string? text, [NotNullWhen(true)] out FriendlyId? id)
    {
        var value = text ?? "";
        var parts = value.Contains(' ') ? value.Split(' ')
            : value.Contains('-') ? value.Split('-')
            : CamelCaseParts(value);
        var lastIsNumber = int.TryParse(parts[^1], NumberStyles.None, CultureInfo.InvariantCulture, out var number);
        var words = lastIsNumber ? parts[..^1] : parts;

        var hasTwoOrThreeWords = 2 <= words.Length && words.Length <= 3;
        var everyWordIsLetters = words.All(word => word.Length > 0 && word.All(char.IsLetter));
        var numberIsPositive = lastIsNumber == false || 1 <= number;

        id = hasTwoOrThreeWords && everyWordIsLetters && numberIsPositive
            ? new FriendlyId(words, lastIsNumber ? number : null)
            : null;
        return id is not null;
    }

    private static string[] CamelCaseParts(string value)
    {
        var match = CamelCaseShape().Match(value);
        return match.Success
            ? match.Groups["part"].Captures.Select(capture => capture.Value.ToLowerInvariant()).ToArray()
            : [value];
    }

    [GeneratedRegex(@"^(?<part>\p{Lu}?\p{Ll}+)+(?<part>[0-9]+)?$")]
    private static partial Regex CamelCaseShape();
}
