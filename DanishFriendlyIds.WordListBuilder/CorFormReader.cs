using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.WordListBuilder;

/// <summary>
/// COR lines:     COR.15497.302.01 · lemma · gloss · grammar label · form · status (N/K/U)
/// COR.EXT lines: COR.EXT.100002.302.01 · lemma · gloss · word class · DDO id · trademark (0/1) · grammar label · form
/// </summary>
public static class CorFormReader
{
    private const char Separator = '\t';

    public static IReadOnlyList<CorForm> ReadCor(TextReader reader) =>
        NonEmptyLines(reader).Select(line => CorLine(line.Text, line.Number)).ToList();

    public static IReadOnlyList<CorForm> ReadCorExt(TextReader reader) =>
        NonEmptyLines(reader).Select(line => CorExtLine(line.Text, line.Number)).ToList();

    private static CorForm CorLine(string text, int lineNumber)
    {
        var fields = Fields(text, expectedCount: 6, lineNumber);
        var idParts = fields[0].Split('.');
        var grammarLabel = fields[3];
        return new CorForm(
            HeadwordId: new WordId($"{idParts[0]}.{idParts[1]}"),
            Lemma: fields[1],
            WordClassLabel: grammarLabel.Split('.')[0],
            GrammarLabel: grammarLabel,
            Form: fields[4],
            Variant: idParts[3],
            Status: fields[5],
            IsTrademark: false);
    }

    private static CorForm CorExtLine(string text, int lineNumber)
    {
        var fields = Fields(text, expectedCount: 8, lineNumber);
        var idParts = fields[0].Split('.');
        return new CorForm(
            HeadwordId: new WordId($"{idParts[0]}.{idParts[1]}.{idParts[2]}"),
            Lemma: fields[1],
            WordClassLabel: fields[3],
            GrammarLabel: fields[6],
            Form: fields[7],
            Variant: idParts[4],
            Status: CorForm.NoStatus,
            IsTrademark: fields[5] == "1");
    }

    private static string[] Fields(string text, int expectedCount, int lineNumber)
    {
        var fields = text.Split(Separator);
        return fields.Length == expectedCount
            ? fields
            : throw new InvalidDataException($"Line {lineNumber}: expected {expectedCount} columns, found {fields.Length}");
    }

    private static IEnumerable<(string Text, int Number)> NonEmptyLines(TextReader reader)
    {
        var number = 0;
        for (var text = reader.ReadLine(); text is not null; text = reader.ReadLine())
        {
            number++;
            if (text.Length > 0)
                yield return (text, number);
        }
    }
}
