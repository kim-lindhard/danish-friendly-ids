using System.Globalization;
using DanishFriendlyIds.Identifiers;
using DanishFriendlyIds.Words;

const int DefaultCount = 50;
IdKind[] presets = [IdKind.Person, IdKind.Case];

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
            var words = WordPoolSelector.Unreviewed(Lexicon.Embedded, WordReview.Embedded, kind);
            return words.Adjectives.Select(word => (Subject: ReviewSubject.Adjective, Word: word, Kind: kind.Name))
                .Concat(words.Nouns.Select(word => (Subject: ReviewSubject.Noun, Word: word, Kind: kind.Name)));
        })
        .GroupBy(entry => (entry.Subject, entry.Word))
        .Select(group => (group.Key.Subject, group.Key.Word, Kinds: string.Join(",", group.Select(entry => entry.Kind))))
        .OrderBy(entry => entry.Subject.Code, StringComparer.Ordinal)
        .ThenBy(entry => entry.Word, StringComparer.Ordinal)
        .ToList();

    Console.WriteLine("word\tword_class\tkinds");
    foreach (var (subject, word, kinds) in unreviewed)
        Console.WriteLine($"{word}\t{subject.Code}\t{kinds}");

    Console.Error.WriteLine($"{unreviewed.Count} unreviewed candidate words");
}
