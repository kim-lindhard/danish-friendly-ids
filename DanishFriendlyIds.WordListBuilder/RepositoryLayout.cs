using System.Diagnostics.CodeAnalysis;

namespace DanishFriendlyIds.WordListBuilder;

public sealed record RepositoryLayout(DirectoryInfo Root)
{
    public const string SolutionFileName = "DanishFriendlyIds.slnx";

    public DirectoryInfo SourcesDirectory => new(Path.Combine(Root.FullName, "data", "sources"));

    public string WordsFile => Path.Combine(Root.FullName, "data", "danish-words.tsv");

    public string SensesFile => Path.Combine(Root.FullName, "data", "danish-senses.tsv");

    public string EmbeddedWordsFile => Path.Combine(Root.FullName, "DanishFriendlyIds", "Data", "danish-words.tsv.gz");

    public string EmbeddedSensesFile => Path.Combine(Root.FullName, "DanishFriendlyIds", "Data", "danish-senses.tsv.gz");

    public string SourcePath(SourceFile source) => Path.Combine(SourcesDirectory.FullName, source.FileName);

    public static bool TryFind(string startDirectory, [NotNullWhen(true)] out RepositoryLayout? layout)
    {
        var root = AncestorsAndSelf(new DirectoryInfo(startDirectory))
            .FirstOrDefault(directory => File.Exists(Path.Combine(directory.FullName, SolutionFileName)));

        layout = root is null ? null : new RepositoryLayout(root);
        return layout is not null;
    }

    private static IEnumerable<DirectoryInfo> AncestorsAndSelf(DirectoryInfo start)
    {
        for (var directory = start; directory is not null; directory = directory.Parent)
            yield return directory;
    }
}
