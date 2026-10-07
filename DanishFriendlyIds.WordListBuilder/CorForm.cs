using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.WordListBuilder;

/// <summary>One inflected form: one line of COR or COR.EXT.</summary>
public sealed record CorForm(
    WordId HeadwordId,
    string Lemma,
    string WordClassLabel,
    string GrammarLabel,
    string Form,
    string Variant,
    string Status,
    bool IsTrademark)
{
    public const string DefiniteAdjectiveLabel = "adj.sg.best";
    public const string PresentParticipleLabel = "vb.præs.part";
    public const string RegulatedStatus = "N";
    public const string NoStatus = "";
}
