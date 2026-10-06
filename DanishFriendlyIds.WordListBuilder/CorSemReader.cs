using System.Globalization;
using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.WordListBuilder;

public static class CorSemReader
{
    private const char Separator = '\t';
    private const char ListSeparator = '|';

    public static IReadOnlyList<CorSemSense> Read(TextReader reader)
    {
        var header = reader.ReadLine() ?? throw new InvalidDataException("COR.SEM is empty");
        var columns = Columns.From(header);

        List<CorSemSense> senses = new();
        var lineNumber = 1;
        for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
        {
            lineNumber++;
            if (line.Length > 0)
                senses.Add(Sense(line.Split(Separator), columns, lineNumber));
        }

        return senses;
    }

    private static CorSemSense Sense(string[] fields, Columns columns, int lineNumber)
    {
        string Field(int index) => fields[index];

        var corTargets = SplitList(Field(columns.CorId)).Select(id => new WordId(id));
        var corExtTargets = SplitList(Field(columns.CorExtId)).Select(id => new WordId(id));

        return new CorSemSense(
            Lemma: Field(columns.Lemma),
            Targets: corTargets.Concat(corExtTargets).ToList(),
            DdoWordClass: Field(columns.WordClass),
            Categories: Field(columns.OntologicalType)
                .Split(['+', ListSeparator], StringSplitOptions.RemoveEmptyEntries)
                .Select(name => ParseCategory(name, lineNumber))
                .ToHashSet(),
            Topics: SplitList(Field(columns.Topic)).Select(code => new Topic(code)).ToHashSet(),
            Sentiment: OptionalInt(Field(columns.Sentiment)),
            Centrality: OptionalInt(Field(columns.Centrality)),
            Restrictions: SplitList(Field(columns.Restriction)).Select(code => ParseRestriction(code, lineNumber)).ToHashSet());
    }

    private static MeaningCategory ParseCategory(string name, int lineNumber) =>
        MeaningCategory.TryFromName(name, out var category)
            ? category
            : throw new InvalidDataException($"COR.SEM line {lineNumber}: unknown ontological type atom '{name}'");

    private static Restriction ParseRestriction(string code, int lineNumber) =>
        Restriction.TryFromCode(code, out var restriction)
            ? restriction
            : throw new InvalidDataException($"COR.SEM line {lineNumber}: unknown restriction '{code}'");

    private static int? OptionalInt(string field) =>
        field.Length == 0 ? null : int.Parse(field, CultureInfo.InvariantCulture);

    private static string[] SplitList(string field) =>
        field.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries);

    private sealed record Columns(
        int CorId, int CorExtId, int Lemma, int WordClass, int OntologicalType,
        int Topic, int Sentiment, int Restriction, int Centrality)
    {
        public static Columns From(string header)
        {
            var names = header.Split(Separator).ToList();

            int IndexOf(string name)
            {
                var index = names.IndexOf(name);
                return index >= 0 ? index : throw new InvalidDataException($"COR.SEM has no column '{name}'");
            }

            return new Columns(
                CorId: IndexOf("COR-basis-id"),
                CorExtId: IndexOf("COR.EXT-id"),
                Lemma: IndexOf("DDO-opslagsord"),
                WordClass: IndexOf("DDO-ordklasse"),
                OntologicalType: IndexOf("ontologisk-type"),
                Topic: IndexOf("emne"),
                Sentiment: IndexOf("sentiment"),
                Restriction: IndexOf("restriktion"),
                Centrality: IndexOf("centralitet"));
        }
    }
}
