using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.Identifiers;

public sealed class Blocklist
{
    public const string EmbeddedResourceName = "DanishFriendlyIds.blocklist.txt";

    private static readonly Lazy<Blocklist> EmbeddedBlocklist = new(LoadEmbedded);

    private readonly HashSet<string> entries;

    public Blocklist(IEnumerable<string> entries)
    {
        this.entries = entries.ToHashSet(StringComparer.Ordinal);
    }

    public static Blocklist Embedded => EmbeddedBlocklist.Value;

    public static Blocklist Empty { get; } = new([]);

    public IReadOnlyCollection<string> Entries => entries;

    public bool Blocks(Word word)
    {
        var definiteFormIsBlocked = word.DefiniteForm is not null && entries.Contains(word.DefiniteForm);
        return entries.Contains(word.Lemma) || definiteFormIsBlocked;
    }

    public static Blocklist Parse(TextReader reader) => new(Lines(reader)
        .Select(line => line.Trim())
        .Where(line => line.Length > 0 && line.StartsWith('#') == false));

    private static IEnumerable<string> Lines(TextReader reader)
    {
        for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
            yield return line;
    }

    private static Blocklist LoadEmbedded()
    {
        using var stream = typeof(Blocklist).Assembly.GetManifestResourceStream(EmbeddedResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {EmbeddedResourceName} is missing");
        using var reader = new StreamReader(stream);
        return Parse(reader);
    }
}
