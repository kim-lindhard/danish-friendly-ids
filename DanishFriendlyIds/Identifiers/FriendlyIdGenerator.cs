using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// Draws random adjective + noun pairs from approved words only, skipping pairs the review blocks.
/// Uniqueness is the caller's: store what you hand out, and use <see cref="TryNext"/> to retry against it.
/// A <see cref="Random"/> passed in is not thread-safe; the default <see cref="Random.Shared"/> is.
/// </summary>
public sealed class FriendlyIdGenerator(Lexicon lexicon, WordReview review, Random random)
{
    public const int MaximumAttempts = 1_000;

    private readonly ConcurrentDictionary<IdKind, WordPools> poolsByKind = new();

    public FriendlyIdGenerator() : this(Random.Shared)
    {
    }

    public FriendlyIdGenerator(Random random) : this(Lexicon.Embedded, WordReview.Embedded, random)
    {
    }

    public WordPools PoolsOf(IdKind kind) =>
        poolsByKind.GetOrAdd(kind, newKind => WordPoolSelector.Select(lexicon, review, newKind));

    public long CapacityOf(IdKind kind)
    {
        var pools = PoolsOf(kind);
        var blockedPairsInPools = review.BlockedPairs.Count(pair =>
            pools.Adjectives.Contains(pair.Adjective) && pools.Nouns.Contains(pair.Noun));
        return pools.Capacity - blockedPairsInPools;
    }

    public FriendlyId Next(IdKind kind)
    {
        var pools = PoolsOf(kind);
        if (pools.IsEmpty)
            throw new InvalidOperationException(EmptyPoolMessage(kind, pools));

        return FirstAllowed(pools, _ => false)
            ?? throw new InvalidOperationException($"{kind.Name}: every pair drawn in {MaximumAttempts} attempts is blocked");
    }

    public bool TryNext(IdKind kind, Func<FriendlyId, bool> isTaken, [NotNullWhen(true)] out FriendlyId? id)
    {
        var pools = PoolsOf(kind);
        id = pools.IsEmpty ? null : FirstAllowed(pools, isTaken);
        return id is not null;
    }

    private FriendlyId? FirstAllowed(WordPools pools, Func<FriendlyId, bool> isTaken) =>
        Enumerable.Range(0, MaximumAttempts)
            .Select(_ => Draw(pools))
            .FirstOrDefault(candidate => review.Blocks(candidate) == false && isTaken(candidate) == false);

    private FriendlyId Draw(WordPools pools) => new(
        pools.Adjectives[random.Next(pools.Adjectives.Count)],
        pools.Nouns[random.Next(pools.Nouns.Count)]);

    private string EmptyPoolMessage(IdKind kind, WordPools pools)
    {
        var unreviewed = WordPoolSelector.Unreviewed(lexicon, review, kind);
        return $"{kind.Name} has {pools.Adjectives.Count} approved adjectives and {pools.Nouns.Count} approved nouns; " +
               $"it needs at least one of each. {unreviewed.Adjectives.Count} adjectives and {unreviewed.Nouns.Count} nouns " +
               "among its candidates are unreviewed: give them a verdict in word-review.tsv.";
    }
}
