using System.Globalization;

namespace DanishFriendlyIds.Words;

/// <summary>
/// Two files. danish-words.tsv has one row per word, and its meaning columns summarise the senses for
/// people reading the file; they are derived, so reading ignores them. danish-senses.tsv has one row per
/// sense per word and is where meaning is read from.
/// </summary>
public static class LexiconTsv
{
    public static IReadOnlyList<string> WordColumns { get; } =
    [
        "id", "lemma", "word_class", "definite_form", "categories", "topics",
        "min_sentiment", "centrality", "restriction", "senses"
    ];

    public static IReadOnlyList<string> SenseColumns { get; } =
    [
        "id", "word_class", "sense", "categories", "topics", "sentiment", "centrality", "restriction"
    ];

    private const char ColumnSeparator = '\t';
    private const char ListSeparator = '|';

    public static void Write(IReadOnlyList<Word> words, TextWriter wordsWriter, TextWriter sensesWriter)
    {
        WriteLines(wordsWriter, WordColumns, words.Select(WordLine));
        WriteLines(sensesWriter, SenseColumns, words.SelectMany(SenseLines));
    }

    public static IReadOnlyList<Word> Read(TextReader wordsReader, TextReader sensesReader)
    {
        var sensesByWord = DataLines(sensesReader, SenseColumns, "senses")
            .Select(line => (Key: (line.Fields[0], line.Fields[1]), Sense: ParseSense(line.Fields, line.Number)))
            .ToLookup(entry => entry.Key, entry => entry.Sense);

        return DataLines(wordsReader, WordColumns, "words")
            .Select(line => ParseWord(line.Fields, line.Number, sensesByWord[(line.Fields[0], line.Fields[2])].ToList()))
            .ToList();
    }

    private static string WordLine(Word word) => string.Join(ColumnSeparator,
        word.Id.Value,
        word.Lemma,
        word.WordClass.CorLabel,
        word.DefiniteForm ?? "",
        JoinSorted(word.Categories.Select(category => category.Name)),
        JoinSorted(word.Topics.Select(topic => topic.Code)),
        OptionalInt(word.MinimumSentiment),
        OptionalInt(word.Centrality),
        JoinSorted(word.Restrictions.Select(restriction => restriction.Code)),
        word.Senses.Count.ToString(CultureInfo.InvariantCulture));

    private static IEnumerable<string> SenseLines(Word word) =>
        word.Senses.Select((sense, index) => string.Join(ColumnSeparator,
            word.Id.Value,
            word.WordClass.CorLabel,
            (index + 1).ToString(CultureInfo.InvariantCulture),
            JoinSorted(sense.Categories.Select(category => category.Name)),
            JoinSorted(sense.Topics.Select(topic => topic.Code)),
            OptionalInt(sense.Sentiment),
            OptionalInt(sense.Centrality),
            JoinSorted(sense.Restrictions.Select(restriction => restriction.Code))));

    private static Word ParseWord(string[] fields, int lineNumber, List<WordSense> senses)
    {
        if (WordClass.TryFromCorLabel(fields[2], out var wordClass) == false)
            throw new InvalidDataException($"words line {lineNumber}: unknown word class '{fields[2]}'");

        var expectedSenses = int.Parse(fields[9], CultureInfo.InvariantCulture);
        if (senses.Count != expectedSenses)
            throw new InvalidDataException($"words line {lineNumber}: {expectedSenses} senses expected, {senses.Count} found");

        return new Word(
            new WordId(fields[0]),
            fields[1],
            wordClass,
            fields[3].Length == 0 ? null : fields[3],
            senses,
            ParseRestrictions(fields[8], lineNumber));
    }

    private static WordSense ParseSense(string[] fields, int lineNumber) => new(
        SplitList(fields[3]).Select(name => ParseCategory(name, lineNumber)).ToHashSet(),
        SplitList(fields[4]).Select(code => new Topic(code)).ToHashSet(),
        ParseOptionalInt(fields[5]),
        ParseOptionalInt(fields[6]),
        ParseRestrictions(fields[7], lineNumber));

    private static IEnumerable<(string[] Fields, int Number)> DataLines(TextReader reader, IReadOnlyList<string> columns, string fileName)
    {
        var header = reader.ReadLine();
        var headerIsExpected = header == string.Join(ColumnSeparator, columns);
        if (headerIsExpected == false)
            throw new InvalidDataException($"Unexpected {fileName} header: {header}");

        var lineNumber = 1;
        for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
        {
            lineNumber++;
            var fields = line.Split(ColumnSeparator);
            if (fields.Length != columns.Count)
                throw new InvalidDataException($"{fileName} line {lineNumber}: expected {columns.Count} columns, found {fields.Length}");

            yield return (fields, lineNumber);
        }
    }

    private static void WriteLines(TextWriter writer, IReadOnlyList<string> columns, IEnumerable<string> lines)
    {
        writer.Write(string.Join(ColumnSeparator, columns));
        writer.Write('\n');
        foreach (var line in lines)
        {
            writer.Write(line);
            writer.Write('\n');
        }
    }

    private static MeaningCategory ParseCategory(string name, int lineNumber) =>
        MeaningCategory.TryFromName(name, out var category)
            ? category
            : throw new InvalidDataException($"senses line {lineNumber}: unknown meaning category '{name}'");

    private static HashSet<Restriction> ParseRestrictions(string field, int lineNumber) =>
        SplitList(field).Select(code => Restriction.TryFromCode(code, out var restriction)
                ? restriction
                : throw new InvalidDataException($"Line {lineNumber}: unknown restriction '{code}'"))
            .ToHashSet();

    private static string OptionalInt(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "";

    private static int? ParseOptionalInt(string field) =>
        field.Length == 0 ? null : int.Parse(field, CultureInfo.InvariantCulture);

    private static string[] SplitList(string field) =>
        field.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries);

    private static string JoinSorted(IEnumerable<string> values) =>
        string.Join(ListSeparator, values.Order(StringComparer.Ordinal));
}
