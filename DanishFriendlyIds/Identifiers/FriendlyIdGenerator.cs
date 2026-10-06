using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// Draws random adjective + noun pairs. Uniqueness is the caller's: store what you hand out, and use
/// <see cref="TryNext"/> to retry against it. A <see cref="Random"/> passed in is not thread-safe;
/// the default <see cref="Random.Shared"/> is.
/// </summary>
public sealed class FriendlyIdGenerator(Lexicon lexicon, Blocklist blocklist, Random random)
{
    public const int MaximumAttempts = 1_000;

    private readonly ConcurrentDictionary<IdKind, WordPools> poolsByKind = new();

    public FriendlyIdGenerator() : this(Random.Shared)
    {
    }

    public FriendlyIdGenerator(Random random) : this(Lexicon.Embedded, Blocklist.Embedded, random)
    {
    }

    public WordPools PoolsOf(IdKind kind) =>
        poolsByKind.GetOrAdd(kind, newKind => WordPoolSelector.Select(lexicon, blocklist, newKind));

    public long CapacityOf(IdKind kind) => PoolsOf(kind).Capacity;

    public FriendlyId Next(IdKind kind)
    {
        var pools = PoolsOf(kind);
        if (pools.IsEmpty)
            throw new InvalidOperationException(
                $"{kind.Name} has {pools.Adjectives.Count} adjectives and {pools.Nouns.Count} nouns after filtering; it needs at least one of each");

        return Draw(pools);
    }

    public bool TryNext(IdKind kind, Func<FriendlyId, bool> isTaken, [NotNullWhen(true)] out FriendlyId? id)
    {
        var pools = PoolsOf(kind);
        id = pools.IsEmpty
            ? null
            : Enumerable.Range(0, MaximumAttempts)
                .Select(_ => Draw(pools))
                .FirstOrDefault(candidate => isTaken(candidate) == false);
        return id is not null;
    }

    private FriendlyId Draw(WordPools pools) => new(
        pools.Adjectives[random.Next(pools.Adjectives.Count)],
        pools.Nouns[random.Next(pools.Nouns.Count)]);
}
