namespace DanishFriendlyIds.Words;

public sealed record WordId(string Value)
{
    public override string ToString() => Value;
}
