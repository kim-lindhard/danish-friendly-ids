using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.WordListBuilder;

public sealed record AssembledWordList(IReadOnlyList<Word> Words, IReadOnlyList<CorSemSense> UnmatchedSenses);

public static class WordListAssembler
{
    public static AssembledWordList Assemble(IEnumerable<CorForm> forms, IEnumerable<CorSemSense> senses)
    {
        var headwords = forms
            .GroupBy(form => (form.HeadwordId, form.WordClassLabel))
            .Select(ToHeadword)
            .ToList();
        var headwordsById = headwords.ToLookup(headword => headword.Id);
        var headwordsByLemmaAndClass = headwords.ToLookup(headword => (headword.Lemma, headword.WordClass.CorLabel));

        var sensesWithTheirHeadwords = senses
            .Select(sense => (Sense: sense, Headwords: MatchingHeadwords(sense, headwordsById, headwordsByLemmaAndClass)))
            .ToList();
        var sensesByHeadword = sensesWithTheirHeadwords
            .SelectMany(match => match.Headwords.Select(headword => (headword, match.Sense)))
            .ToLookup(pair => pair.headword, pair => pair.Sense);

        var words = headwords
            .Select(headword => ToWord(headword, sensesByHeadword[headword].ToList()))
            .OrderBy(word => word.Id.Value, StringComparer.Ordinal)
            .ThenBy(word => word.WordClass.CorLabel, StringComparer.Ordinal)
            .ToList();
        var unmatchedSenses = sensesWithTheirHeadwords
            .Where(match => match.Headwords.Count == 0)
            .Select(match => match.Sense)
            .ToList();

        return new AssembledWordList(words, unmatchedSenses);
    }

    private static Headword ToHeadword(IGrouping<(WordId HeadwordId, string WordClassLabel), CorForm> forms)
    {
        if (WordClass.TryFromCorLabel(forms.Key.WordClassLabel, out var wordClass) == false)
            throw new InvalidDataException($"{forms.Key.HeadwordId}: unknown word class '{forms.Key.WordClassLabel}'");

        return new Headword(
            forms.Key.HeadwordId,
            forms.First().Lemma,
            wordClass,
            wordClass == WordClass.Adjective ? FirstRegulatedForm(forms, CorForm.DefiniteAdjectiveLabel) : null,
            wordClass == WordClass.Verb ? FirstRegulatedForm(forms, CorForm.PresentParticipleLabel) : null,
            forms.Any(form => form.IsTrademark));
    }

    private static string? FirstRegulatedForm(IEnumerable<CorForm> forms, string grammarLabel) =>
        forms
            .Where(form => form.GrammarLabel == grammarLabel)
            .Where(form => form.Status == CorForm.RegulatedStatus || form.Status == CorForm.NoStatus)
            .OrderBy(form => form.Variant, StringComparer.Ordinal)
            .Select(form => form.Form)
            .FirstOrDefault();

    /// <summary>
    /// COR.SEM 1.0 was linked to an older COR: some senses have no link, and some point at numbers
    /// COR 1.5 has since dropped or renumbered. Those fall back to the one headword, if exactly one,
    /// with the same lemma and word class.
    /// </summary>
    private static List<Headword> MatchingHeadwords(
        CorSemSense sense,
        ILookup<WordId, Headword> headwordsById,
        ILookup<(string Lemma, string CorLabel), Headword> headwordsByLemmaAndClass)
    {
        var linked = sense.Targets
            .SelectMany(target => HeadwordsForTarget(sense, headwordsById[target].ToList()))
            .Distinct()
            .ToList();
        if (linked.Count > 0)
            return linked;

        var sameLemmaAndClass = headwordsByLemmaAndClass[(sense.Lemma, sense.CorWordClassLabel)].ToList();
        return sameLemmaAndClass.Count == 1 ? sameLemmaAndClass : [];
    }

    private static IEnumerable<Headword> HeadwordsForTarget(CorSemSense sense, List<Headword> candidates)
    {
        var sameWordClass = candidates
            .Where(candidate => candidate.WordClass.CorLabel == sense.CorWordClassLabel)
            .ToList();

        return sameWordClass.Count > 0 ? sameWordClass
            : candidates.Count == 1 ? candidates
            : [];
    }

    private static Word ToWord(Headword headword, List<CorSemSense> senses)
    {
        IEnumerable<Restriction> trademark = headword.IsTrademark ? [Restriction.Trademark] : [];
        var restrictionsOnEverySense = Restriction.All
            .Where(restriction => senses.Count > 0 && senses.All(sense => sense.Restrictions.Contains(restriction)));

        return new Word(
            headword.Id,
            headword.Lemma,
            headword.WordClass,
            headword.DefiniteForm,
            senses.Select(ToWordSense).ToList(),
            restrictionsOnEverySense.Concat(trademark).ToHashSet())
        {
            PresentParticiple = headword.PresentParticiple
        };
    }

    private static WordSense ToWordSense(CorSemSense sense) =>
        new(sense.Categories, sense.Topics, sense.Sentiment, sense.Centrality, sense.Restrictions);

    private sealed record Headword(
        WordId Id, string Lemma, WordClass WordClass, string? DefiniteForm, string? PresentParticiple, bool IsTrademark);
}
