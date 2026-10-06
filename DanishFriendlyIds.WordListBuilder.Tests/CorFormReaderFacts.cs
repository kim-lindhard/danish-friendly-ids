using DanishFriendlyIds.Words;
using Xunit;

namespace DanishFriendlyIds.WordListBuilder;

public class CorFormReaderFacts
{
    [Fact]
    public void A_cor_line_is_split_into_headword_class_grammar_form_variant_and_status()
    {
        // Arrange
        var forms = Fixtures.CorForms();

        // Act
        var definiteGlad = forms.Single(form => form.Lemma == "glad" && form.GrammarLabel == "adj.sg.best");

        // Assert
        Assert.Equal(new WordId("COR.15497"), definiteGlad.HeadwordId);
        Assert.Equal("adj", definiteGlad.WordClassLabel);
        Assert.Equal("glade", definiteGlad.Form);
        Assert.Equal("01", definiteGlad.Variant);
        Assert.Equal("N", definiteGlad.Status);
        Assert.False(definiteGlad.IsTrademark);
    }

    [Fact]
    public void A_cor_ext_line_keeps_the_ext_number_and_reads_the_trademark_flag()
    {
        // Arrange
        var forms = Fixtures.CorExtForms();

        // Act
        var trademarked = forms.Where(form => form.HeadwordId == new WordId("COR.EXT.145381")).ToList();
        var notTrademarked = forms.Where(form => form.HeadwordId == new WordId("COR.EXT.100002")).ToList();

        // Assert
        Assert.NotEmpty(trademarked);
        Assert.All(trademarked, form => Assert.True(form.IsTrademark));
        Assert.All(notTrademarked, form => Assert.False(form.IsTrademark));
        Assert.All(notTrademarked, form => Assert.Equal("adj", form.WordClassLabel));
    }

    [Fact]
    public void A_line_with_the_wrong_number_of_columns_is_rejected_with_its_line_number()
    {
        // Arrange
        using var reader = new StringReader("COR.15497.302.01\tglad\t\tadj.sg.best\tglade\tN\n\nCOR.15497.303.01\tglad\n");

        // Act
        var error = Assert.Throws<InvalidDataException>(() => CorFormReader.ReadCor(reader));

        // Assert
        Assert.Contains("Line 3", error.Message);
    }
}
