using System.IO.Compression;
using System.Text;

namespace DanishFriendlyIds.Words;

public sealed class Lexicon
{
    public const string EmbeddedWordsResourceName = "DanishFriendlyIds.danish-words.tsv.gz";
    public const string EmbeddedSensesResourceName = "DanishFriendlyIds.danish-senses.tsv.gz";

    private static readonly Lazy<Lexicon> EmbeddedLexicon = new(LoadEmbedded);

    private readonly ILookup<string, Word> wordsByLemma;

    public Lexicon(IReadOnlyList<Word> words)
    {
        Words = words;
        wordsByLemma = words.ToLookup(word => word.Lemma, StringComparer.Ordinal);
    }

    public static Lexicon Embedded => EmbeddedLexicon.Value;

    public IReadOnlyList<Word> Words { get; }

    public IEnumerable<Word> Find(string lemma) => wordsByLemma[lemma];

    public IEnumerable<Word> Of(WordClass wordClass) => Words.Where(word => word.WordClass == wordClass);

    private static Lexicon LoadEmbedded()
    {
        using var words = OpenEmbedded(EmbeddedWordsResourceName);
        using var senses = OpenEmbedded(EmbeddedSensesResourceName);
        return new Lexicon(LexiconTsv.Read(words, senses));
    }

    private static StreamReader OpenEmbedded(string resourceName)
    {
        var compressed = typeof(Lexicon).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource {resourceName} is missing");
        return new StreamReader(new GZipStream(compressed, CompressionMode.Decompress), Encoding.UTF8);
    }
}
