using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// Draws random identifiers from words approved in the kind's own review list, never repeating a word and
/// skipping any identifier that contains a blocked pair. Uniqueness is the caller's: store what you hand
/// out, and use <see cref="TryNext(IdKind, IdFormat, Func{FriendlyId, bool}, out FriendlyId?)"/> to retry
/// against it. A <see cref="Random"/> passed in is not thread-safe; the default <see cref="Random.Shared"/> is.
/// </summary>
public sealed class FriendlyIdGenerator(Lexicon lexicon, ReviewLists reviews, Random random)
{
    public const int MaximumAttempts = 1_000;

    private readonly ConcurrentDictionary<IdKind, WordPools> poolsByKind = new();

    public FriendlyIdGenerator() : this(Random.Shared)
    {
    }

    public FriendlyIdGenerator(Random random) : this(Lexicon.Embedded, ReviewLists.Embedded, random)
    {
    }

    public WordPools PoolsOf(IdKind kind) =>
        poolsByKind.GetOrAdd(kind, newKind => WordPoolSelector.Select(lexicon, reviews, newKind));

    public long CapacityOf(IdKind kind) => CapacityOf(kind, IdFormat.TwoWords);

    public long CapacityOf(IdKind kind, IdFormat format) =>
        IdentifierSpace.Count(PoolsOf(kind), format, reviews.For(kind.Vocabulary));

    public FriendlyId Next(IdKind kind) => Next(kind, IdFormat.TwoWords);

    public FriendlyId Next(IdKind kind, IdFormat format)
    {
        var pools = PoolsOf(kind);
        if (pools.CanMake(format) == false)
            throw new InvalidOperationException(EmptyPoolMessage(kind, format, pools));

        return FirstAllowed(kind, format, pools, _ => false)
            ?? throw new InvalidOperationException($"{kind.Name}: every identifier drawn in {MaximumAttempts} attempts is blocked");
    }

    public bool TryNext(IdKind kind, Func<FriendlyId, bool> isTaken, [NotNullWhen(true)] out FriendlyId? id) =>
        TryNext(kind, IdFormat.TwoWords, isTaken, out id);

    public bool TryNext(IdKind kind, IdFormat format, Func<FriendlyId, bool> isTaken, [NotNullWhen(true)] out FriendlyId? id)
    {
        var pools = PoolsOf(kind);
        id = pools.CanMake(format) ? FirstAllowed(kind, format, pools, isTaken) : null;
        return id is not null;
    }

    private FriendlyId? FirstAllowed(IdKind kind, IdFormat format, WordPools pools, Func<FriendlyId, bool> isTaken)
    {
        var review = reviews.For(kind.Vocabulary);
        return Enumerable.Range(0, MaximumAttempts)
            .Select(_ => Draw(format, pools))
            .FirstOrDefault(candidate => IdentifierSpace.IsAllowed(candidate, review) && isTaken(candidate) == false);
    }

    private FriendlyId Draw(IdFormat format, WordPools pools)
    {
        var words = format.WordCount == 2
            ? FriendlyId.Of(Pick(pools.FirstWords), Pick(pools.Nouns))
            : FriendlyId.Of(Pick(pools.Adjectives), Pick(pools.Participles), Pick(pools.Nouns));
        return format.MaximumNumber is { } maximum ? words.WithNumber(random.Next(1, maximum + 1)) : words;
    }

    private string Pick(IReadOnlyList<string> pool) => pool[random.Next(pool.Count)];

    private string EmptyPoolMessage(IdKind kind, IdFormat format, WordPools pools)
    {
        var unreviewed = WordPoolSelector.Unreviewed(lexicon, reviews, kind);
        return $"{kind.Name} cannot make {format} identifiers: it has {pools.Adjectives.Count} approved adjectives, " +
               $"{pools.Participles.Count} -ende words and {pools.Nouns.Count} nouns. " +
               $"{unreviewed.Adjectives.Count} adjectives, {unreviewed.Participles.Count} -ende words and " +
               $"{unreviewed.Nouns.Count} nouns among its candidates are unreviewed: " +
               $"give them a verdict in review-{kind.Vocabulary.Name}.tsv.";
    }
}
