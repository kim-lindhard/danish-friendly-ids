using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.WordListBuilder;

/// <summary>One sense in COR.SEM, pointing at one or more COR headwords or one COR.EXT headword.</summary>
public sealed record CorSemSense(
    string Lemma,
    IReadOnlyList<WordId> Targets,
    string DdoWordClass,
    IReadOnlySet<MeaningCategory> Categories,
    IReadOnlySet<Topic> Topics,
    int? Sentiment,
    int? Centrality,
    IReadOnlySet<Restriction> Restrictions)
{
    /// <summary>DDO writes "sb.", "sb. pl.", "talord (mængdetal)"; COR writes "sb", "talord".</summary>
    public string CorWordClassLabel => DdoWordClass.Split(' ', '.')[0];
}
