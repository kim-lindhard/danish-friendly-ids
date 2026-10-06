using System.Text.RegularExpressions;
using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

public static partial class WordPoolSelector
{
    private static readonly Topic Sexual = new("sex");

    public static WordPools Select(Lexicon lexicon, Blocklist blocklist, IdKind kind) => new(
        Pool(lexicon.Of(WordClass.Adjective), word => word.DefiniteForm, kind.Adjectives, kind, blocklist),
        Pool(lexicon.Of(WordClass.Noun), word => word.Lemma, kind.Nouns, kind, blocklist));

    public static bool IsSafe(Word word, IdKind kind, Blocklist blocklist)
    {
        var topics = word.Topics;
        var hasNoRestriction = word.Restrictions.Count == 0;
        var hasNoSexualSense = topics.Contains(Sexual) == false;
        var hasNoExcludedTopic = topics.Overlaps(kind.ExcludedTopics) == false;
        var isCentralEnough = kind.MinimumCentrality <= (word.Centrality ?? 0);
        var isPositiveEnough = kind.MinimumSentiment <= (word.MinimumSentiment ?? 0);
        var isNotBlocked = blocklist.Blocks(word) == false;

        return word.HasMeaning && hasNoRestriction && hasNoSexualSense && hasNoExcludedTopic
            && isCentralEnough && isPositiveEnough && isNotBlocked;
    }

    public static bool HasIdentifierShape(string shownForm) => IdentifierShape().IsMatch(shownForm);

    private static List<string> Pool(
        IEnumerable<Word> words, Func<Word, string?> shownForm, SenseFilter filter, IdKind kind, Blocklist blocklist) =>
        words
            .SelectMany(word => shownForm(word) is { } shown ? [(Word: word, Shown: shown)] : Array.Empty<(Word Word, string Shown)>())
            .Where(candidate => HasIdentifierShape(candidate.Shown))
            .Where(candidate => IsSafe(candidate.Word, kind, blocklist))
            .Where(candidate => filter.MatchesASenseOf(candidate.Word))
            .Select(candidate => candidate.Shown)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

    [GeneratedRegex("^[a-zæøå]{3,12}$", RegexOptions.CultureInvariant)]
    private static partial Regex IdentifierShape();
}
