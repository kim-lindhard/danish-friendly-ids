using System.Diagnostics.CodeAnalysis;

namespace DanishFriendlyIds.Words;

/// <summary>
/// One atom of a COR.SEM ontological type. COR.SEM writes a sense's type as atoms joined by
/// <c>+</c> (Vehicle+Artifact+Object), and a word gets every atom of every one of its senses.
/// </summary>
public sealed record MeaningCategory(string Name)
{
    public static readonly MeaningCategory Abstract = new("Abstract");
    public static readonly MeaningCategory Act = new("Act");
    public static readonly MeaningCategory Animal = new("Animal");
    public static readonly MeaningCategory Artifact = new("Artifact");
    public static readonly MeaningCategory Artwork = new("Artwork");
    public static readonly MeaningCategory BodyPart = new("BodyPart");
    public static readonly MeaningCategory Building = new("Building");
    public static readonly MeaningCategory Cause = new("Cause");
    public static readonly MeaningCategory Colour = new("Colour");
    public static readonly MeaningCategory Comestible = new("Comestible");
    public static readonly MeaningCategory Communication = new("Communication");
    public static readonly MeaningCategory Condition = new("Condition");
    public static readonly MeaningCategory Container = new("Container");
    public static readonly MeaningCategory Creature = new("Creature");
    public static readonly MeaningCategory Domain = new("Domain");
    public static readonly MeaningCategory Event = new("Event");
    public static readonly MeaningCategory Existence = new("Existence");
    public static readonly MeaningCategory Experience = new("Experience");
    public static readonly MeaningCategory FirstOrderEntity = new("1stOrderEntity");
    public static readonly MeaningCategory Form = new("Form");
    public static readonly MeaningCategory Furniture = new("Furniture");
    public static readonly MeaningCategory Garment = new("Garment");
    public static readonly MeaningCategory GeopoliticalPlace = new("GeopoliticalPlace");
    public static readonly MeaningCategory Group = new("Group");
    public static readonly MeaningCategory Human = new("Human");
    public static readonly MeaningCategory ImageRepresentation = new("Imagerepresentation");
    public static readonly MeaningCategory Institution = new("Institution");
    public static readonly MeaningCategory Instrument = new("Instrument");
    public static readonly MeaningCategory LanguageRepresentation = new("LanguageRepresentation");
    public static readonly MeaningCategory Liquid = new("Liquid");
    public static readonly MeaningCategory Living = new("Living");
    public static readonly MeaningCategory Location = new("Location");
    public static readonly MeaningCategory Mental = new("Mental");
    public static readonly MeaningCategory MoneyRepresentation = new("MoneyRepresentation");
    public static readonly MeaningCategory Natural = new("Natural");
    public static readonly MeaningCategory Object = new("Object");
    public static readonly MeaningCategory Occupation = new("Occupation");
    public static readonly MeaningCategory Part = new("Part");
    public static readonly MeaningCategory Phenomenal = new("Phenomenal");
    public static readonly MeaningCategory Physical = new("Physical");
    public static readonly MeaningCategory Place = new("Place");
    public static readonly MeaningCategory Plant = new("Plant");
    public static readonly MeaningCategory Possession = new("Possession");
    public static readonly MeaningCategory Property = new("Property");
    public static readonly MeaningCategory Purpose = new("Purpose");
    public static readonly MeaningCategory Quantity = new("Quantity");
    public static readonly MeaningCategory Relation = new("Relation");
    public static readonly MeaningCategory SecondOrderEntity = new("2ndOrderEntity");
    public static readonly MeaningCategory Social = new("Social");
    public static readonly MeaningCategory Static = new("Static");
    public static readonly MeaningCategory Substance = new("Substance");
    public static readonly MeaningCategory Time = new("Time");
    public static readonly MeaningCategory Top = new("Top");
    public static readonly MeaningCategory Vehicle = new("Vehicle");
    public static readonly MeaningCategory Weather = new("Weather");
    public static readonly MeaningCategory DegreeAdverb = new("ADV_Degree");
    public static readonly MeaningCategory DirectionAdverb = new("ADV_Direction");
    public static readonly MeaningCategory MannerAdverb = new("ADV_Manner");
    public static readonly MeaningCategory NegationAdverb = new("ADV_Negation");
    public static readonly MeaningCategory PlaceAdverb = new("ADV_Place");
    public static readonly MeaningCategory SentenceAdverb = new("ADV_Sentence");
    public static readonly MeaningCategory TimeAdverb = new("ADV_Time");

    public static IReadOnlyList<MeaningCategory> All { get; } =
    [
        Abstract, Act, Animal, Artifact, Artwork, BodyPart, Building, Cause, Colour, Comestible,
        Communication, Condition, Container, Creature, Domain, Event, Existence, Experience,
        FirstOrderEntity, Form, Furniture, Garment, GeopoliticalPlace, Group, Human, ImageRepresentation,
        Institution, Instrument, LanguageRepresentation, Liquid, Living, Location, Mental,
        MoneyRepresentation, Natural, Object, Occupation, Part, Phenomenal, Physical, Place, Plant,
        Possession, Property, Purpose, Quantity, Relation, SecondOrderEntity, Social, Static, Substance,
        Time, Top, Vehicle, Weather, DegreeAdverb, DirectionAdverb, MannerAdverb, NegationAdverb,
        PlaceAdverb, SentenceAdverb, TimeAdverb
    ];

    public static bool TryFromName(string name, [NotNullWhen(true)] out MeaningCategory? category)
    {
        category = All.FirstOrDefault(candidate => candidate.Name == name);
        return category is not null;
    }

    public override string ToString() => Name;
}
