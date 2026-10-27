namespace Treachery.Shared;

public static class Membership
{
    public static bool IsActive(DateTimeOffset? validUntil) => validUntil > DateTimeOffset.Now;
}
