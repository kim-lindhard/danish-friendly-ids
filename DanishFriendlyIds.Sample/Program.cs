using System.Globalization;
using DanishFriendlyIds.Identifiers;
using DanishFriendlyIds.Words;

const string Usage = """
    Usage: dotnet run --project DanishFriendlyIds.Sample -- [count=50] [seed] [two|three] [maximum number, 0 = none] [danish|ascii|url] [common|less]
           dotnet run --project DanishFriendlyIds.Sample -- capacity
           dotnet run --project DanishFriendlyIds.Sample -- unreviewed
           dotnet run --project DanishFriendlyIds.Sample -- resolve person|object <identifier in any style> [common|less]
    """;
const int DefaultCount = 50;
var presets = IdKind.Presets;

if (args.Length > 0 && args[0] == "unreviewed")
{
    PrintUnreviewed();
    return 0;
}

if (args.Length is 3 or 4 && args[0] == "resolve" && (args.Length == 3 || args[3] is "common" or "less"))
{
    var kind = presets
        .Where(preset => string.Equals(preset.Name, args[1], StringComparison.OrdinalIgnoreCase))
        .Select(preset => args.Length == 4 && args[3] == "less" ? preset.WithLessCommonWords() : preset)
        .FirstOrDefault();
    var resolver = new FriendlyIdGenerator();
    if (kind is not null && resolver.TryResolve(kind, args[2], out var resolved))
    {
        Console.WriteLine($"{resolved}  |  {resolved.ToString(IdStyle.Ascii)}  |  {resolved.ToString(IdStyle.UrlSlug)}");
        return 0;
    }

    Console.Error.WriteLine($"Not an identifier {args[1]} could have made: {args[2]}");
    return 1;
}

if (args.Length > 0 && args[0] == "capacity")
{
    PrintCapacity(new FriendlyIdGenerator());
    return 0;
}

var count = DefaultCount;
var seed = 0;
var maximumNumber = 0;
IdStyle? style = IdStyle.Danish;
var argumentsAreValid =
    (args.Length < 1 || int.TryParse(args[0], CultureInfo.InvariantCulture, out count)) &&
    (args.Length < 2 || int.TryParse(args[1], CultureInfo.InvariantCulture, out seed)) &&
    (args.Length < 3 || args[2] is "two" or "three") &&
    (args.Length < 4 || args[3] == "0" || int.TryParse(args[3], CultureInfo.InvariantCulture, out maximumNumber) && 2 <= maximumNumber) &&
    (args.Length < 5 || IdStyle.TryFromName(args[4], out style)) &&
    (args.Length < 6 || args[5] is "common" or "less");

if (argumentsAreValid == false)
{
    Console.Error.WriteLine(Usage);
    return 1;
}

var words = args.Length >= 3 && args[2] == "three" ? IdFormat.ThreeWords : IdFormat.TwoWords;
var format = 2 <= maximumNumber ? words.WithNumber(maximumNumber) : words;
var ids = new FriendlyIdGenerator(args.Length >= 2 ? new Random(seed) : Random.Shared);

var includeLessCommon = args.Length >= 6 && args[5] == "less";
foreach (var kind in presets.Select(preset => includeLessCommon ? preset.WithLessCommonWords() : preset))
{
    var pools = ids.PoolsOf(kind);
    Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
        $"{kind.Description}, {format}: {pools.Adjectives.Count:N0} adjectives, {pools.Participles.Count:N0} -ende words, " +
        $"{pools.Nouns.Count:N0} nouns = {ids.CapacityOf(kind, format):N0} identifiers"));

    foreach (var id in Enumerable.Range(0, count).Select(_ => ids.Next(kind, format)))
        Console.WriteLine($"  {id.ToString(style ?? IdStyle.Danish)}");

    Console.WriteLine();
}

return 0;

void PrintCapacity(FriendlyIdGenerator generator)
{
    IdFormat[] formats =
    [
        IdFormat.TwoWords, IdFormat.TwoWords.WithNumber(9), IdFormat.TwoWords.WithNumber(99),
        IdFormat.ThreeWords, IdFormat.ThreeWords.WithNumber(9), IdFormat.ThreeWords.WithNumber(99)
    ];

    Console.WriteLine($"{"",-28}" + string.Concat(formats.Select(format => $"{format,22}")));
    foreach (var kind in IdKind.PresetsWithWordChoices)
        Console.WriteLine($"{kind.Description,-28}" + string.Concat(formats.Select(format =>
            string.Create(CultureInfo.InvariantCulture, $"{generator.CapacityOf(kind, format),22:N0}"))));
}

void PrintUnreviewed()
{
    var unreviewed = IdKind.PresetsWithWordChoices
        .SelectMany(kind =>
        {
            var candidates = WordPoolSelector.Unreviewed(Lexicon.Embedded, ReviewLists.Embedded, kind);
            var list = kind.Vocabulary.Name;
            return candidates.Adjectives.Select(word => (List: list, Subject: ReviewSubject.Adjective, Word: word))
                .Concat(candidates.Participles.Select(word => (List: list, Subject: ReviewSubject.Participle, Word: word)))
                .Concat(candidates.Nouns.Select(word => (List: list, Subject: ReviewSubject.Noun, Word: word)));
        })
        .Distinct()
        .OrderBy(entry => entry.List, StringComparer.Ordinal)
        .ThenBy(entry => entry.Subject.Code, StringComparer.Ordinal)
        .ThenBy(entry => entry.Word, StringComparer.Ordinal)
        .ToList();

    Console.WriteLine("list\tword\tword_class");
    foreach (var (list, subject, word) in unreviewed)
        Console.WriteLine($"{list}\t{word}\t{subject.Code}");

    Console.Error.WriteLine($"{unreviewed.Count} unreviewed candidate words");
}
