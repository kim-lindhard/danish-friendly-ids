namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// Counts the identifiers a kind can make in a format, exactly: every combination of the pools, minus
/// those that repeat a word or contain a blocked pair, times the number choices.
/// </summary>
public static class IdentifierSpace
{
    public static long Count(WordPools pools, IdFormat format, WordReview review) =>
        format.WordCount == 2
            ? TwoWordCount(pools, review) * format.NumberChoices
            : ThreeWordCount(pools, review) * format.NumberChoices;

    public static bool IsAllowed(FriendlyId id, WordReview review)
    {
        var repeatsAWord = id.Words.Distinct(StringComparer.Ordinal).Count() < id.Words.Count;
        return repeatsAWord == false && review.Blocks(id) == false;
    }

    private static long TwoWordCount(WordPools pools, WordReview review)
    {
        var nouns = pools.Nouns.ToHashSet(StringComparer.Ordinal);
        var excluded = pools.FirstWords
            .SelectMany(first => pools.Nouns.Select(noun => (First: first, Noun: noun)))
            .Count(pair => pair.First == pair.Noun || review.Blocks(new WordPair(pair.First, pair.Noun)));
        return (long)pools.FirstWords.Count * nouns.Count - excluded;
    }

    private static long ThreeWordCount(WordPools pools, WordReview review)
    {
        var adjectives = pools.Adjectives.ToHashSet(StringComparer.Ordinal);
        var participles = pools.Participles.ToHashSet(StringComparer.Ordinal);
        var nouns = pools.Nouns.ToHashSet(StringComparer.Ordinal);

        var repeated = RepeatedWordTriples(adjectives, participles, nouns);
        var blocked = review.BlockedPairs.SelectMany(pair => TriplesContaining(pair, pools, adjectives, participles, nouns));
        var excluded = repeated.Concat(blocked).ToHashSet().Count;

        return (long)adjectives.Count * participles.Count * nouns.Count - excluded;
    }

    private static IEnumerable<(string, string, string)> RepeatedWordTriples(
        HashSet<string> adjectives, HashSet<string> participles, HashSet<string> nouns) =>
        adjectives.Intersect(participles).SelectMany(word => nouns.Select(noun => (word, word, noun)))
            .Concat(adjectives.Intersect(nouns).SelectMany(word => participles.Select(participle => (word, participle, word))))
            .Concat(participles.Intersect(nouns).SelectMany(word => adjectives.Select(adjective => (adjective, word, word))));

    private static IEnumerable<(string, string, string)> TriplesContaining(
        WordPair pair, WordPools pools, HashSet<string> adjectives, HashSet<string> participles, HashSet<string> nouns)
    {
        var asAdjectiveAndParticiple = adjectives.Contains(pair.First) && participles.Contains(pair.Second)
            ? pools.Nouns.Select(noun => (pair.First, pair.Second, noun))
            : [];
        var asAdjectiveAndNoun = adjectives.Contains(pair.First) && nouns.Contains(pair.Second)
            ? pools.Participles.Select(participle => (pair.First, participle, pair.Second))
            : [];
        var asParticipleAndNoun = participles.Contains(pair.First) && nouns.Contains(pair.Second)
            ? pools.Adjectives.Select(adjective => (adjective, pair.First, pair.Second))
            : [];
        return asAdjectiveAndParticiple.Concat(asAdjectiveAndNoun).Concat(asParticipleAndNoun);
    }
}
