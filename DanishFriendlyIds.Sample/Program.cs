using System.Globalization;
using DanishFriendlyIds.Identifiers;
using DanishFriendlyIds.Words;

const string Usage = """
    Usage: dotnet run --project DanishFriendlyIds.Sample -- [count=50] [seed] [two|three] [maximum number]
           dotnet run --project DanishFriendlyIds.Sample -- capacity
           dotnet run --project DanishFriendlyIds.Sample -- unreviewed
    """;
const int DefaultCount = 50;
var presets = IdKind.Presets;

if (args.Length > 0 && args[0] == "unreviewed")
{
    PrintUnreviewed();
    return 0;
}

if (args.Length > 0 && args[0] == "capacity")
{
    PrintCapacity(new FriendlyIdGenerator());
    return 0;
}

var count = DefaultCount;
var seed = 0;
var maximumNumber = 0;
var argumentsAreValid =
    (args.Length < 1 || int.TryParse(args[0], CultureInfo.InvariantCulture, out count)) &&
    (args.Length < 2 || int.TryParse(args[1], CultureInfo.InvariantCulture, out seed)) &&
    (args.Length < 3 || args[2] is "two" or "three") &&
    (args.Length < 4 || int.TryParse(args[3], CultureInfo.InvariantCulture, out maximumNumber) && 2 <= maximumNumber);

if (argumentsAreValid == false)
{
    Console.Error.WriteLine(Usage);
    return 1;
}

var words = args.Length >= 3 && args[2] == "three" ? IdFormat.ThreeWords : IdFormat.TwoWords;
var format = args.Length >= 4 ? words.WithNumber(maximumNumber) : words;
var ids = new FriendlyIdGenerator(args.Length >= 2 ? new Random(seed) : Random.Shared);

foreach (var kind in presets)
{
    var pools = ids.PoolsOf(kind);
    Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
        $"{kind.Name}, {format}: {pools.Adjectives.Count:N0} adjectives, {pools.Participles.Count:N0} -ende words, " +
        $"{pools.Nouns.Count:N0} nouns = {ids.CapacityOf(kind, format):N0} identifiers"));

    foreach (var id in Enumerable.Range(0, count).Select(_ => ids.Next(kind, format)))
        Console.WriteLine($"  {id}");

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

    Console.WriteLine($"{"",-8}" + string.Concat(formats.Select(format => $"{format,22}")));
    foreach (var kind in presets)
        Console.WriteLine($"{kind.Name,-8}" + string.Concat(formats.Select(format =>
            string.Create(CultureInfo.InvariantCulture, $"{generator.CapacityOf(kind, format),22:N0}"))));
}

void PrintUnreviewed()
{
    var unreviewed = presets
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
