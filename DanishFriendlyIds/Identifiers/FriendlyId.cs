using System.Diagnostics.CodeAnalysis;

namespace DanishFriendlyIds.Identifiers;

public sealed record FriendlyId(string Adjective, string Noun)
{
    public override string ToString() => $"{Adjective} {Noun}";

    public static bool TryParse(string? text, [NotNullWhen(true)] out FriendlyId? id)
    {
        var words = (text ?? "").Split(' ');
        var isTwoWords = words.Length == 2 && words.All(word => word.Length > 0);

        id = isTwoWords ? new FriendlyId(words[0], words[1]) : null;
        return id is not null;
    }
}
