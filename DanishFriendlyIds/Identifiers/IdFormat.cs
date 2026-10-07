namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// The shape of an identifier: two words (adjective or -ende word + noun, "glade danser"), or three
/// (adjective + -ende word + noun, "glade dansende pilot"), optionally followed by a number from 1 to
/// <see cref="MaximumNumber"/> ("glade danser 7").
/// </summary>
public sealed record IdFormat
{
    public static readonly IdFormat TwoWords = new(2, null);
    public static readonly IdFormat ThreeWords = new(3, null);

    private IdFormat(int wordCount, int? maximumNumber)
    {
        WordCount = wordCount;
        MaximumNumber = maximumNumber;
    }

    public int WordCount { get; }

    public int? MaximumNumber { get; }

    public long NumberChoices => MaximumNumber ?? 1;

    public IdFormat WithNumber(int maximum) => 2 <= maximum
        ? new IdFormat(WordCount, maximum)
        : throw new ArgumentOutOfRangeException(nameof(maximum), maximum, "A number suffix needs a maximum of at least 2");

    public IdFormat WithoutNumber() => new(WordCount, null);

    public override string ToString() =>
        MaximumNumber is { } maximum ? $"{WordCount} words + 1–{maximum}" : $"{WordCount} words";
}
