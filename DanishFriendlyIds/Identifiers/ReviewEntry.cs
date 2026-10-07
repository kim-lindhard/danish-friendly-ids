using System.Diagnostics.CodeAnalysis;

namespace DanishFriendlyIds.Identifiers;

public sealed record ReviewVerdict(string Code)
{
    public static readonly ReviewVerdict Approved = new("approved");
    public static readonly ReviewVerdict Rejected = new("rejected");
    public static readonly ReviewVerdict Unreviewed = new("unreviewed");

    public static IReadOnlyList<ReviewVerdict> Written { get; } = [Approved, Rejected];

    public static bool TryFromCode(string code, [NotNullWhen(true)] out ReviewVerdict? verdict)
    {
        verdict = Written.FirstOrDefault(candidate => candidate.Code == code);
        return verdict is not null;
    }

    public override string ToString() => Code;
}

/// <summary>What a review line is about: an adjective's definite form, a verb's -ende form, a noun, or two words that may not appear together.</summary>
public sealed record ReviewSubject(string Code)
{
    public static readonly ReviewSubject Adjective = new("adj");
    public static readonly ReviewSubject Participle = new("part");
    public static readonly ReviewSubject Noun = new("sb");
    public static readonly ReviewSubject Pair = new("pair");

    public static IReadOnlyList<ReviewSubject> All { get; } = [Adjective, Participle, Noun, Pair];

    public static bool TryFromCode(string code, [NotNullWhen(true)] out ReviewSubject? subject)
    {
        subject = All.FirstOrDefault(candidate => candidate.Code == code);
        return subject is not null;
    }

    public override string ToString() => Code;
}

public sealed record ReviewEntry(string Word, ReviewSubject Subject, ReviewVerdict Verdict, string Reason);
