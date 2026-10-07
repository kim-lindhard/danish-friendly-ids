using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

/// <summary>
/// The hand-checked verdicts for one <see cref="Vocabulary"/>. Only approved words are drawn; a word
/// with no verdict is treated as rejected, so a new COR release cannot put an unread word into an
/// identifier. A rejected pair blocks two words that are fine alone but insulting together.
/// </summary>
public sealed class WordReview
{
    public static IReadOnlyList<string> Columns { get; } = ["word", "word_class", "verdict", "reason"];

    private const char Separator = '\t';

    private readonly Dictionary<(ReviewSubject Subject, string Word), ReviewEntry> entries;

    public WordReview(IEnumerable<ReviewEntry> entries)
    {
        this.entries = new Dictionary<(ReviewSubject, string), ReviewEntry>();
        foreach (var entry in entries)
        {
            if (this.entries.TryAdd((entry.Subject, entry.Word), entry) == false)
                throw new InvalidDataException($"'{entry.Word}' ({entry.Subject}) is reviewed twice");
        }

        BlockedPairs = this.entries.Values
            .Where(entry => entry.Subject == ReviewSubject.Pair && entry.Verdict == ReviewVerdict.Rejected)
            .Select(entry => FriendlyId.TryParse(entry.Word, out var pair)
                ? pair
                : throw new InvalidDataException($"Pair '{entry.Word}' is not two words"))
            .ToList();
    }

    public static WordReview Empty { get; } = new([]);

    public IReadOnlyCollection<ReviewEntry> Entries => entries.Values;

    public IReadOnlyList<FriendlyId> BlockedPairs { get; }

    public ReviewVerdict Verdict(WordClass wordClass, string shownForm)
    {
        var subject = wordClass == WordClass.Adjective ? ReviewSubject.Adjective
            : wordClass == WordClass.Noun ? ReviewSubject.Noun
            : null;

        return subject is not null && entries.TryGetValue((subject, shownForm), out var entry)
            ? entry.Verdict
            : ReviewVerdict.Unreviewed;
    }

    public bool IsApproved(WordClass wordClass, string shownForm) =>
        Verdict(wordClass, shownForm) == ReviewVerdict.Approved;

    public bool Blocks(FriendlyId id) =>
        entries.TryGetValue((ReviewSubject.Pair, id.ToString()), out var entry) && entry.Verdict == ReviewVerdict.Rejected;

    public static WordReview Parse(TextReader reader)
    {
        var header = reader.ReadLine();
        var headerIsExpected = header == string.Join(Separator, Columns);
        if (headerIsExpected == false)
            throw new InvalidDataException($"Unexpected word-review header: {header}");

        List<ReviewEntry> parsed = new();
        var lineNumber = 1;
        for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
        {
            lineNumber++;
            if (line.Length > 0)
                parsed.Add(ParseLine(line, lineNumber));
        }

        return new WordReview(parsed);
    }

    private static ReviewEntry ParseLine(string line, int lineNumber)
    {
        var fields = line.Split(Separator);
        if (fields.Length != Columns.Count)
            throw new InvalidDataException($"word-review line {lineNumber}: expected {Columns.Count} columns, found {fields.Length}");

        if (ReviewSubject.TryFromCode(fields[1], out var subject) == false)
            throw new InvalidDataException($"word-review line {lineNumber}: unknown word_class '{fields[1]}' (adj, sb or pair)");

        if (ReviewVerdict.TryFromCode(fields[2], out var verdict) == false)
            throw new InvalidDataException($"word-review line {lineNumber}: unknown verdict '{fields[2]}' (approved or rejected)");

        var rejectionHasNoReason = verdict == ReviewVerdict.Rejected && fields[3].Trim().Length == 0;
        if (rejectionHasNoReason)
            throw new InvalidDataException($"word-review line {lineNumber}: '{fields[0]}' is rejected without a reason");

        return new ReviewEntry(fields[0], subject, verdict, fields[3]);
    }

    public static WordReview LoadEmbedded(string resourceName)
    {
        using var stream = typeof(WordReview).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource {resourceName} is missing");
        using var reader = new StreamReader(stream);
        return Parse(reader);
    }
}
