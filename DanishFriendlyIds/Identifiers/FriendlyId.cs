using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DanishFriendlyIds.Identifiers;

/// <summary>Two or three words, the last a noun, optionally followed by a number: "glade dansende pilot 42".</summary>
public sealed record FriendlyId
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
        var everyWordIsPresent = words.All(word => word.Length > 0 && word.Contains(' ') == false);
        return hasTwoOrThreeWords && everyWordIsPresent
            ? new FriendlyId(words.ToArray(), null)
            : throw new ArgumentException("An identifier is two or three words", nameof(words));
    }

    public FriendlyId WithNumber(int number) => 1 <= number
        ? new FriendlyId(Words, number)
        : throw new ArgumentOutOfRangeException(nameof(number), number, "The number must be at least 1");

    public bool Equals(FriendlyId? other) =>
        other is not null && Words.SequenceEqual(other.Words, StringComparer.Ordinal) && Number == other.Number;

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(ToString());

    public override string ToString()
    {
        var words = string.Join(' ', Words);
        return Number is { } number ? $"{words} {number.ToString(CultureInfo.InvariantCulture)}" : words;
    }

    public static bool TryParse(string? text, [NotNullWhen(true)] out FriendlyId? id)
    {
        var parts = (text ?? "").Split(' ');
        var lastIsNumber = int.TryParse(parts[^1], NumberStyles.None, CultureInfo.InvariantCulture, out var number);
        var words = lastIsNumber ? parts[..^1] : parts;

        var hasTwoOrThreeWords = 2 <= words.Length && words.Length <= 3;
        var wordsAreWords = words.All(word => word.Length > 0 && word.Any(char.IsDigit) == false);
        var numberIsPositive = lastIsNumber == false || 1 <= number;

        id = hasTwoOrThreeWords && wordsAreWords && numberIsPositive
            ? new FriendlyId(words, lastIsNumber ? number : null)
            : null;
        return id is not null;
    }
}
