using System.Text.RegularExpressions;
using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// Candidates are the words the rules allow for a kind; pools are the candidates the review approved.
/// </summary>
public static partial class WordPoolSelector
{
    private static readonly IReadOnlySet<Topic> TopicsExcludedForEveryKind = new HashSet<Topic> { new("sex"), new("etn") };

    public static WordPools Candidates(Lexicon lexicon, IdKind kind) => new(
        Candidates(lexicon.Of(WordClass.Adjective), word => word.DefiniteForm, kind.Adjectives, kind),
        Candidates(lexicon.Of(WordClass.Noun), word => word.Lemma, kind.Nouns, kind));

    public static WordPools Select(Lexicon lexicon, WordReview review, IdKind kind) =>
        WithVerdict(Candidates(lexicon, kind), review, ReviewVerdict.Approved);

    public static WordPools Unreviewed(Lexicon lexicon, WordReview review, IdKind kind) =>
        WithVerdict(Candidates(lexicon, kind), review, ReviewVerdict.Unreviewed);

    public static bool FollowsTheRules(Word word, IdKind kind)
    {
        var topics = word.Topics;
        var hasNoRestriction = word.Restrictions.Count == 0;
        var hasNoTopicExcludedForEveryKind = topics.Overlaps(TopicsExcludedForEveryKind) == false;
        var hasNoTopicExcludedForThisKind = topics.Overlaps(kind.ExcludedTopics) == false;
        var isCentralEnough = kind.MinimumCentrality <= (word.Centrality ?? 0);
        var isPositiveEnough = word.Senses.All(sense => kind.MinimumSentiment <= (sense.Sentiment ?? 0));

        return word.HasMeaning && hasNoRestriction && hasNoTopicExcludedForEveryKind && hasNoTopicExcludedForThisKind
            && isCentralEnough && isPositiveEnough;
    }

    public static bool HasIdentifierShape(string shownForm) => IdentifierShape().IsMatch(shownForm);

    private static WordPools WithVerdict(WordPools candidates, WordReview review, ReviewVerdict verdict) => new(
        candidates.Adjectives.Where(adjective => review.Verdict(WordClass.Adjective, adjective) == verdict).ToList(),
        candidates.Nouns.Where(noun => review.Verdict(WordClass.Noun, noun) == verdict).ToList());

    private static List<string> Candidates(
        IEnumerable<Word> words, Func<Word, string?> shownForm, SenseFilter filter, IdKind kind) =>
        words
            .SelectMany(word => shownForm(word) is { } shown ? [(Word: word, Shown: shown)] : Array.Empty<(Word Word, string Shown)>())
            .Where(candidate => HasIdentifierShape(candidate.Shown))
            .Where(candidate => FollowsTheRules(candidate.Word, kind))
            .Where(candidate => filter.MatchesASenseOf(candidate.Word))
            .Select(candidate => candidate.Shown)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

    [GeneratedRegex("^[a-zæøå]{3,12}$", RegexOptions.CultureInvariant)]
    private static partial Regex IdentifierShape();
}
