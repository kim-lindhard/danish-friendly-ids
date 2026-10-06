namespace DanishFriendlyIds.WordListBuilder;

/// <summary>Real rows cut from COR 1.5.1.0, COR.EXT 1.0 and COR.SEM 1.0 (all CC0).</summary>
internal static class Fixtures
{
    public static TextReader Open(string fileName) =>
        File.OpenText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));

    public static IReadOnlyList<CorForm> CorForms()
    {
        using var reader = Open("cor.tsv");
        return CorFormReader.ReadCor(reader);
    }

    public static IReadOnlyList<CorForm> CorExtForms()
    {
        using var reader = Open("corext.tsv");
        return CorFormReader.ReadCorExt(reader);
    }

    public static IReadOnlyList<CorSemSense> CorSemSenses()
    {
        using var reader = Open("corsem.tsv");
        return CorSemReader.Read(reader);
    }

    public static AssembledWordList Assembled() =>
        WordListAssembler.Assemble(CorForms().Concat(CorExtForms()), CorSemSenses());
}
