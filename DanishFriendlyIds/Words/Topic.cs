namespace DanishFriendlyIds.Words;

/// <summary>A subject-field code from Den Danske Ordbog, as carried by COR.SEM (zoo, med, mad, spo …).</summary>
public sealed record Topic(string Code)
{
    public override string ToString() => Code;
}
