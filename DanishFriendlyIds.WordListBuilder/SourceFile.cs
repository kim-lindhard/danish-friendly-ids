namespace DanishFriendlyIds.WordListBuilder;

/// <summary>
/// The pinned upstream files. A checksum mismatch stops the build: ordregister.dk publishes new
/// versions under new names, so a changed file under an old name is something to look at by hand.
/// </summary>
public sealed record SourceFile(string FileName, Uri Url, string Sha256)
{
    public static readonly SourceFile Cor = new(
        "cor1.5.1.0.tsv",
        new Uri("https://ordregister.dk/files/cor1.5.1.0.tsv"),
        "1c4d1c06bd676e66be8e8c0af68615c8a892472a05af3b261ba8a9d1f2d2b82b");

    public static readonly SourceFile CorExt = new(
        "corext1.0.tsv",
        new Uri("https://ordregister.dk/files/corext1.0.tsv"),
        "b4caa89974664feb037a87e00e16ebdd6240c901e108ba03cd7fbd45f460564f");

    public static readonly SourceFile CorSem = new(
        "cor.sem.1.0.tsv",
        new Uri("https://ordregister.dk/files/cor.sem.1.0.tsv"),
        "864a6d5704aa49914eb4ce634799678a20d843d11e8cce8310b4736bf5908724");

    public static IReadOnlyList<SourceFile> All { get; } = [Cor, CorExt, CorSem];
}
