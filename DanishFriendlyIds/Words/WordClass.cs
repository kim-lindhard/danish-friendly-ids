using System.Diagnostics.CodeAnalysis;

namespace DanishFriendlyIds.Words;

public sealed record WordClass(string CorLabel, string EnglishName)
{
    public static readonly WordClass Noun = new("sb", "noun");
    public static readonly WordClass Adjective = new("adj", "adjective");
    public static readonly WordClass Verb = new("vb", "verb");
    public static readonly WordClass Adverb = new("adv", "adverb");
    public static readonly WordClass ProperNoun = new("prop", "proper noun");
    public static readonly WordClass Abbreviation = new("fork", "abbreviation");
    public static readonly WordClass FixedExpressionWord = new("iflerord", "word used only in a fixed expression");
    public static readonly WordClass Prefix = new("præfiks", "prefix");
    public static readonly WordClass MultiWordExpression = new("flerord", "multi-word expression");
    public static readonly WordClass Interjection = new("udråbsord", "interjection");
    public static readonly WordClass Suffix = new("suffiks", "suffix");
    public static readonly WordClass Numeral = new("talord", "numeral");
    public static readonly WordClass Preposition = new("præp", "preposition");
    public static readonly WordClass Conjunction = new("konj", "conjunction");
    public static readonly WordClass Onomatopoeia = new("lydord", "onomatopoeia");
    public static readonly WordClass Pronoun = new("pron", "pronoun");
    public static readonly WordClass Symbol = new("symbol", "symbol");
    public static readonly WordClass RomanNumeral = new("romertal", "Roman numeral");
    public static readonly WordClass FormalSubject = new("formsubj", "formal subject");
    public static readonly WordClass Article = new("art", "article");
    public static readonly WordClass InfinitiveMarker = new("infmærke", "infinitive marker");

    public static IReadOnlyList<WordClass> All { get; } =
    [
        Noun, Adjective, Verb, Adverb, ProperNoun, Abbreviation, FixedExpressionWord, Prefix,
        MultiWordExpression, Interjection, Suffix, Numeral, Preposition, Conjunction, Onomatopoeia,
        Pronoun, Symbol, RomanNumeral, FormalSubject, Article, InfinitiveMarker
    ];

    public static bool TryFromCorLabel(string corLabel, [NotNullWhen(true)] out WordClass? wordClass)
    {
        wordClass = All.FirstOrDefault(candidate => candidate.CorLabel == corLabel);
        return wordClass is not null;
    }

    public override string ToString() => CorLabel;
}
