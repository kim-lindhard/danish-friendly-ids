namespace DanishFriendlyIds.Identifiers;

/// <summary>One <see cref="WordReview"/> per <see cref="Vocabulary"/>.</summary>
public sealed record ReviewLists(WordReview People, WordReview Objects)
{
    private static readonly Lazy<ReviewLists> EmbeddedLists = new(() => new ReviewLists(
        WordReview.LoadEmbedded(Vocabulary.People.ReviewResourceName),
        WordReview.LoadEmbedded(Vocabulary.Objects.ReviewResourceName)));

    public static ReviewLists Embedded => EmbeddedLists.Value;

    public WordReview For(Vocabulary vocabulary) =>
        vocabulary == Vocabulary.People ? People
        : vocabulary == Vocabulary.Objects ? Objects
        : throw new ArgumentOutOfRangeException(nameof(vocabulary), vocabulary, "Unknown vocabulary");
}
