using System.IO.Compression;
using System.Text;
using DanishFriendlyIds.WordListBuilder;
using DanishFriendlyIds.Words;

if (RepositoryLayout.TryFind(Directory.GetCurrentDirectory(), out var layout) == false)
{
    Console.Error.WriteLine($"Run this from inside the repository: no {RepositoryLayout.SolutionFileName} above {Directory.GetCurrentDirectory()}");
    return 1;
}

using var httpClient = new HttpClient();
var downloader = new SourceDownloader(httpClient, layout.SourcesDirectory);
foreach (var source in SourceFile.All)
{
    var file = await downloader.EnsureDownloadedAsync(source, CancellationToken.None);
    if (SourceDownloader.HasPinnedChecksum(file, source) == false)
    {
        Console.Error.WriteLine(
            $"{file.FullName} does not match its pinned SHA-256. If upstream changed on purpose, " +
            $"review the new file, then update the checksum in {nameof(SourceFile)}.cs");
        return 1;
    }

    Console.WriteLine($"Verified {source.FileName}");
}

IReadOnlyList<CorForm> ReadForms(SourceFile source, Func<TextReader, IReadOnlyList<CorForm>> read)
{
    using var reader = File.OpenText(layout.SourcePath(source));
    return read(reader);
}

var forms = ReadForms(SourceFile.Cor, CorFormReader.ReadCor)
    .Concat(ReadForms(SourceFile.CorExt, CorFormReader.ReadCorExt))
    .ToList();

IReadOnlyList<CorSemSense> senses;
using (var reader = File.OpenText(layout.SourcePath(SourceFile.CorSem)))
    senses = CorSemReader.Read(reader);

var wordList = WordListAssembler.Assemble(forms, senses);
var utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

StreamWriter PlainWriter(string path) => new(path, append: false, utf8WithoutBom);

StreamWriter CompressedWriter(string path)
{
    new FileInfo(path).Directory?.Create();
    return new StreamWriter(new GZipStream(File.Create(path), CompressionLevel.SmallestSize), utf8WithoutBom);
}

await using (var wordsWriter = PlainWriter(layout.WordsFile))
await using (var sensesWriter = PlainWriter(layout.SensesFile))
    LexiconTsv.Write(wordList.Words, wordsWriter, sensesWriter);

await using (var wordsWriter = CompressedWriter(layout.EmbeddedWordsFile))
await using (var sensesWriter = CompressedWriter(layout.EmbeddedSensesFile))
    LexiconTsv.Write(wordList.Words, wordsWriter, sensesWriter);

foreach (var written in new[] { layout.WordsFile, layout.SensesFile, layout.EmbeddedWordsFile, layout.EmbeddedSensesFile })
    Console.WriteLine($"Wrote {written}");
Console.WriteLine();
Console.Write(BuildReport.Describe(wordList));
return 0;
