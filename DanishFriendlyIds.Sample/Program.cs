using System.Globalization;
using DanishFriendlyIds.Identifiers;

const int DefaultCount = 50;

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
    return 1;
}

var ids = new FriendlyIdGenerator(seedIsGiven ? new Random(seed) : Random.Shared);

foreach (var kind in new[] { IdKind.Person, IdKind.Case })
{
    var pools = ids.PoolsOf(kind);
    Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
        $"{kind.Name}: {pools.Adjectives.Count:N0} adjectives × {pools.Nouns.Count:N0} nouns = {pools.Capacity:N0} identifiers"));

    foreach (var id in Enumerable.Range(0, count).Select(_ => ids.Next(kind)))
        Console.WriteLine($"  {id}");

    Console.WriteLine();
}

return 0;
