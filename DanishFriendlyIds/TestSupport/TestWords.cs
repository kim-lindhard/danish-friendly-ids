using DanishFriendlyIds.Identifiers;
using DanishFriendlyIds.Words;

namespace DanishFriendlyIds.TestSupport;

/// <summary>Hand-built words, review lists and generators for Facts; removed from Release builds.</summary>
public static class TestWords
{
    public static WordSense Sense(params MeaningCategory[] categories) =>
        new(categories.ToHashSet(), new HashSet<Topic>(), null, 3, new HashSet<Restriction>());

    public static Word Adjective(string lemma, string definiteForm, params WordSense[] senses) =>
        new(new WordId($"TEST.{lemma}"), lemma, WordClass.Adjective, definiteForm, senses, new HashSet<Restriction>());

    public static Word Verb(string lemma, string presentParticiple, params WordSense[] senses) =>
        new(new WordId($"TEST.{lemma}"), lemma, WordClass.Verb, null, senses, new HashSet<Restriction>())
        {
            PresentParticiple = presentParticiple
        };

    public static Word Noun(string lemma, params WordSense[] senses) =>
        new(new WordId($"TEST.{lemma}"), lemma, WordClass.Noun, null, senses, new HashSet<Restriction>());

    public static readonly Word Happy = Adjective("glad", "glade", Sense(MeaningCategory.Property, MeaningCategory.Mental));
    public static readonly Word Calm = Adjective("rolig", "rolige", Sense(MeaningCategory.Property, MeaningCategory.Mental));
    public static readonly Word Red = Adjective("rød", "røde",
        Sense(MeaningCategory.Property, MeaningCategory.Physical, MeaningCategory.Condition),
        Sense(MeaningCategory.Property, MeaningCategory.Colour));
    public static readonly Word Flushed = Adjective("rødmosset", "rødmossede",
        Sense(MeaningCategory.Property, MeaningCategory.Physical, MeaningCategory.Condition));
    public static readonly Word Dancing = Verb("danse", "dansende", Sense(MeaningCategory.Act, MeaningCategory.Physical));
    public static readonly Word Singing = Verb("synge", "syngende", Sense(MeaningCategory.Act, MeaningCategory.Communication));
    public static readonly Word Dancer = Noun("danser", Sense(MeaningCategory.Human, MeaningCategory.Object));
    public static readonly Word Pilot = Noun("pilot", Sense(MeaningCategory.Human, MeaningCategory.Object, MeaningCategory.Occupation));
    public static readonly Word Baker = Noun("bager", Sense(MeaningCategory.Human, MeaningCategory.Object, MeaningCategory.Occupation));
    public static readonly Word Cyclist = Noun("cyklist", Sense(MeaningCategory.Human, MeaningCategory.Object));
    public static readonly Word Tractor = Noun("traktor", Sense(MeaningCategory.Vehicle, MeaningCategory.Artifact, MeaningCategory.Object));
    public static readonly Word Box = Noun("kasse", Sense(MeaningCategory.Container, MeaningCategory.Artifact, MeaningCategory.Object));

    public static readonly IdKind Vehicles = new("Køretøj", Vocabulary.Objects, [MeaningCategory.Colour], [MeaningCategory.Vehicle]);

    public static ReviewEntry Approved(Word word) =>
        word.WordClass == WordClass.Adjective ? new(word.DefiniteForm ?? "", ReviewSubject.Adjective, ReviewVerdict.Approved, "")
        : word.WordClass == WordClass.Verb ? new(word.PresentParticiple ?? "", ReviewSubject.Participle, ReviewVerdict.Approved, "")
        : new(word.Lemma, ReviewSubject.Noun, ReviewVerdict.Approved, "");

    public static ReviewEntry BlockedPair(string first, string second) =>
        new($"{first} {second}", ReviewSubject.Pair, ReviewVerdict.Rejected, "test");

    public static FriendlyIdGenerator Generator(
        IEnumerable<ReviewEntry> peopleList, IEnumerable<ReviewEntry> objectsList, params Word[] words) =>
        new(new Lexicon(words), new ReviewLists(new WordReview(peopleList), new WordReview(objectsList)), new Random(7));

    public static FriendlyIdGenerator PeopleListOnly(IEnumerable<ReviewEntry> peopleList, params Word[] words) =>
        Generator(peopleList, [], words);

    public static FriendlyIdGenerator ApprovingAll(params Word[] words) =>
        Generator(words.Select(Approved), words.Select(Approved), words);

    public static List<FriendlyId> Drawn(FriendlyIdGenerator ids, IdKind kind) => Drawn(ids, kind, IdFormat.TwoWords);

    public static List<FriendlyId> Drawn(FriendlyIdGenerator ids, IdKind kind, IdFormat format) =>
        Enumerable.Range(0, 500).Select(_ => ids.Next(kind, format)).Distinct().ToList();
}
