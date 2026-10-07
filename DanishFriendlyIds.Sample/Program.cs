using System.Globalization;
using DanishFriendlyIds.Identifiers;
using DanishFriendlyIds.Words;

const int DefaultCount = 50;
var presets = IdKind.Presets;

if (args.Length > 0 && args[0] == "unreviewed")
{
    PrintUnreviewed();
    return 0;
}

var countIsGiven = args.Length > 0;
var seedIsGiven = args.Length > 1;
var count = DefaultCount;
var seed = 0;
var argumentsAreValid =
    (countIsGiven == false || int.TryParse(args[0], CultureInfo.InvariantCulture, out count)) &&
    (seedIsGiven == false || int.TryParse(args[1], CultureInfo.InvariantCulture, out seed));

if (argumentsAreValid == false)
{
    Console.Error.WriteLine("Usage: dotnet run --project DanishFriendlyIds.Sample -- [count=50] [seed]");
    Console.Error.WriteLine("       dotnet run --project DanishFriendlyIds.Sample -- unreviewed");
    return 1;
}

var ids = new FriendlyIdGenerator(seedIsGiven ? new Random(seed) : Random.Shared);

foreach (var kind in presets)
{
    var pools = ids.PoolsOf(kind);
    Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
        $"{kind.Name}: {pools.Adjectives.Count:N0} adjectives × {pools.Nouns.Count:N0} nouns = {ids.CapacityOf(kind):N0} identifiers"));

    foreach (var id in Enumerable.Range(0, count).Select(_ => ids.Next(kind)))
        Console.WriteLine($"  {id}");

    Console.WriteLine();
}

return 0;

void PrintUnreviewed()
{
    var unreviewed = presets
        .SelectMany(kind =>
        {
            var words = WordPoolSelector.Unreviewed(Lexicon.Embedded, ReviewLists.Embedded, kind);
            return words.Adjectives.Select(word => (List: kind.Vocabulary.Name, Subject: ReviewSubject.Adjective, Word: word))
                .Concat(words.Nouns.Select(word => (List: kind.Vocabulary.Name, Subject: ReviewSubject.Noun, Word: word)));
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
