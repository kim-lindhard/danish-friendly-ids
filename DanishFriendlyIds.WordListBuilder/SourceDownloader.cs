using System.Security.Cryptography;

namespace DanishFriendlyIds.WordListBuilder;

public sealed class SourceDownloader(HttpClient httpClient, DirectoryInfo sourcesDirectory)
{
    public async Task<FileInfo> EnsureDownloadedAsync(SourceFile source, CancellationToken cancellationToken)
    {
        var file = new FileInfo(Path.Combine(sourcesDirectory.FullName, source.FileName));
        if (file.Exists)
            return file;

        sourcesDirectory.Create();
        var partialFile = file.FullName + ".partial";
        await using (var download = await httpClient.GetStreamAsync(source.Url, cancellationToken))
        await using (var output = File.Create(partialFile))
            await download.CopyToAsync(output, cancellationToken);

        File.Move(partialFile, file.FullName, overwrite: true);
        file.Refresh();
        return file;
    }

    public static bool HasPinnedChecksum(FileInfo file, SourceFile source)
    {
        using var stream = file.OpenRead();
        return Convert.ToHexStringLower(SHA256.HashData(stream)) == source.Sha256;
    }
}
