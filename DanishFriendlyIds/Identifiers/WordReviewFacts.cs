using DanishFriendlyIds.Words;
using Xunit;

namespace DanishFriendlyIds.Identifiers;

public class WordReviewFacts
{
    private const string Header = "word\tword_class\tverdict\treason\n";

    private static WordReview Parsed(string body) => WordReview.Parse(new StringReader(Header + body));

    private static InvalidDataException Rejection(string body) =>
        Assert.Throws<InvalidDataException>(() => Parsed(body));

    [Fact]
    public void Verdicts_are_read_per_word_class()
    {
        // Act
        var review = Parsed("glade\tadj\tapproved\t\nfange\tsb\trejected\tmisfortune\n");

        // Assert
        Assert.Equal(ReviewVerdict.Approved, review.Verdict(WordClass.Adjective, "glade"));
        Assert.Equal(ReviewVerdict.Rejected, review.Verdict(WordClass.Noun, "fange"));
        Assert.Equal(ReviewVerdict.Unreviewed, review.Verdict(WordClass.Noun, "glade"));
        Assert.Equal(ReviewVerdict.Unreviewed, review.Verdict(WordClass.Verb, "glade"));
    }

    [Fact]
    public void A_rejected_pair_blocks_that_identifier()
    {
        // Act
        var review = Parsed("tomme tønde\tpair\trejected\tidiom for a blowhard\n");

        // Assert
        Assert.True(review.Blocks(new FriendlyId("tomme", "tønde")));
        Assert.False(review.Blocks(new FriendlyId("tomme", "kasse")));
        Assert.Equal([new FriendlyId("tomme", "tønde")], review.BlockedPairs);
    }

    [Theory]
    [InlineData("glade\tadj\tapproved\n", "expected 4 columns")]
    [InlineData("glade\tverb\tapproved\t\n", "unknown word_class")]
    [InlineData("glade\tadj\tmaybe\t\n", "unknown verdict")]
    [InlineData("fange\tsb\trejected\t \n", "without a reason")]
    [InlineData("tomme\tpair\trejected\tnot a pair\n", "not two words")]
    [InlineData("glade\tadj\tapproved\t\nglade\tadj\trejected\tchanged my mind\n", "reviewed twice")]
    public void A_malformed_review_is_rejected(string body, string expectedMessage)
    {
        // Act
        var error = Rejection(body);

        // Assert
        Assert.Contains(expectedMessage, error.Message);
    }

    [Fact]
    public void A_file_with_another_header_is_rejected()
    {
        // Act
        var error = Assert.Throws<InvalidDataException>(() => WordReview.Parse(new StringReader("word\tverdict\n")));

        // Assert
        Assert.Contains("header", error.Message);
    }

    [Fact]
    public void Every_reviewed_word_is_a_word_the_lexicon_can_show()
    {
        // Arrange
        var lexicon = Lexicon.Embedded;
        var adjectives = lexicon.Of(WordClass.Adjective).Select(word => word.DefiniteForm).OfType<string>().ToHashSet();
        var nouns = lexicon.Of(WordClass.Noun).Select(word => word.Lemma).ToHashSet();

        // Act
        var unknown = WordReview.Embedded.Entries
            .Where(entry => entry.Subject == ReviewSubject.Adjective && adjectives.Contains(entry.Word) == false
                            || entry.Subject == ReviewSubject.Noun && nouns.Contains(entry.Word) == false)
            .Select(entry => entry.Word)
            .ToList();

        // Assert
        Assert.Empty(unknown);
    }

    [Fact]
    public void Every_blocked_pair_is_made_of_approved_words()
    {
        // Arrange
        var review = WordReview.Embedded;

        // Act
        var pairsWithUnapprovedWords = review.BlockedPairs
            .Where(pair => review.IsApproved(WordClass.Adjective, pair.Adjective) == false
                           || review.IsApproved(WordClass.Noun, pair.Noun) == false)
            .ToList();

        // Assert
        Assert.Empty(pairsWithUnapprovedWords);
    }

    [Theory]
    [MemberData(nameof(Presets))]
    public void Every_candidate_of_a_preset_has_a_verdict(string kindName)
    {
        // Arrange
        var kind = kindName == IdKind.Person.Name ? IdKind.Person : IdKind.Case;

        // Act
        var unreviewed = WordPoolSelector.Unreviewed(Lexicon.Embedded, WordReview.Embedded, kind);

        // Assert
        Assert.Empty(unreviewed.Adjectives);
        Assert.Empty(unreviewed.Nouns);
    }

    public static TheoryData<string> Presets => new(IdKind.Person.Name, IdKind.Case.Name);
}
