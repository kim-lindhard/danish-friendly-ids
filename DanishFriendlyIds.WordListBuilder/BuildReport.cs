using System.Globalization;
using System.Text;
using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.WordListBuilder;

public static class BuildReport
{
    private const int UnmatchedSensesShown = 20;

    public static string Describe(AssembledWordList wordList)
    {
        var words = wordList.Words;
        var adjectivesWithoutDefiniteForm = words
            .Where(word => word.WordClass == WordClass.Adjective && word.DefiniteForm is null)
            .ToList();

        var report = new StringBuilder();
        report.AppendLine($"Words (headword + class):          {Number(words.Count)}");
        report.AppendLine($"Headwords:                         {Number(words.Select(word => word.Id).Distinct().Count())}");
        report.AppendLine($"  from COR:                        {Number(DistinctHeadwordsWithPrefix(words, "COR.") - DistinctHeadwordsWithPrefix(words, "COR.EXT."))}");
        report.AppendLine($"  from COR.EXT:                    {Number(DistinctHeadwordsWithPrefix(words, "COR.EXT."))}");
        report.AppendLine($"Headwords with meaning (COR.SEM):  {Number(words.Where(word => word.HasMeaning).Select(word => word.Id).Distinct().Count())}");
        report.AppendLine($"Senses on words:                   {Number(words.Sum(word => word.Senses.Count))}");
        report.AppendLine($"Adjectives without definite form:  {Number(adjectivesWithoutDefiniteForm.Count)}");
        report.AppendLine($"COR.SEM senses not matched:        {Number(wordList.UnmatchedSenses.Count)}");
        report.AppendLine("  Expected: meanings of words that are not COR/COR.EXT headwords in that word class,");
        report.AppendLine("  such as compounds (barbersalon), comparatives (bedre) and words dropped from COR (bededagsferie).");
        report.AppendLine();
        report.AppendLine("Per word class:");

        var countsPerClass = WordClass.All
            .Select(wordClass => (WordClass: wordClass, Count: words.Count(word => word.WordClass == wordClass)))
            .OrderByDescending(entry => entry.Count);
        foreach (var (wordClass, count) in countsPerClass)
            report.AppendLine($"  {wordClass.CorLabel,-10} {wordClass.EnglishName,-40} {Number(count),8}");

        var someUnmatched = wordList.UnmatchedSenses.Take(UnmatchedSensesShown).ToList();
        if (someUnmatched.Count > 0)
        {
            report.AppendLine();
            report.AppendLine($"First {someUnmatched.Count} unmatched senses:");
            foreach (var sense in someUnmatched)
                report.AppendLine($"  {sense.Lemma} ({sense.DdoWordClass}) → {string.Join(", ", sense.Targets)}");
        }

        return report.ToString();
    }

    private static int DistinctHeadwordsWithPrefix(IReadOnlyList<Word> words, string prefix) =>
        words.Select(word => word.Id.Value).Where(id => id.StartsWith(prefix, StringComparison.Ordinal)).Distinct().Count();

    private static string Number(int value) => value.ToString("N0", CultureInfo.InvariantCulture);
}
