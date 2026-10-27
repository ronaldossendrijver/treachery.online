using Microsoft.AspNetCore.Http;
using Stripe;

namespace Treachery.Server;

/// <summary>
/// Applies Stripe subscription events to user memberships. Every event sets an absolute date, so duplicate or
/// retried events are harmless.
/// </summary>
public class MembershipService(TreacheryContext db)
{
    public const string InvoicePaid = "invoice.paid";
    public const string SubscriptionDeleted = "customer.subscription.deleted";

    /// <summary>
    /// Extra time on top of a paid period, so a renewal that arrives a bit late does not interrupt the membership.
    /// </summary>
    public static readonly TimeSpan GracePeriod = TimeSpan.FromDays(2);

    public static async Task<IResult> HandleWebhookAsync(HttpRequest request, IConfiguration configuration, TreacheryContext db)
    {
        var secret = configuration["StripeWebhookSecret"];
        if (string.IsNullOrEmpty(secret))
            return Results.NotFound();

        var json = await new StreamReader(request.Body).ReadToEndAsync();

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, request.Headers["Stripe-Signature"], secret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException)
        {
            return Results.BadRequest();
        }

        await new MembershipService(db).HandleEventAsync(stripeEvent);
        return Results.Ok();
    }

    /// <summary>
    /// Returns the id of the user whose membership was updated, or null if the event was ignored.
    /// </summary>
    public async Task<int?> HandleEventAsync(Event stripeEvent) => stripeEvent.Data.Object switch
    {
        Invoice invoice when stripeEvent.Type == InvoicePaid && invoice.Lines?.Data is { Count: > 0 } lines =>
            await SetMembershipAsync(invoice.CustomerId, Utc(lines.Max(l => l.Period.End)) + GracePeriod, onlyExtend: true),

        Subscription subscription when stripeEvent.Type == SubscriptionDeleted =>
            await SetMembershipAsync(subscription.CustomerId, subscription.EndedAt is { } endedAt ? Utc(endedAt) : DateTimeOffset.UtcNow, onlyExtend: false),

        _ => null
    };

    private static DateTimeOffset Utc(DateTime dateTime) => new(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));

    private async Task<int?> SetMembershipAsync(string? customerId, DateTimeOffset validUntil, bool onlyExtend)
    {
        if (string.IsNullOrEmpty(customerId))
            return null;

        var user = await db.Users.FirstOrDefaultAsync(u => u.StripeCustomerId == customerId);
        if (user == null)
        {
            Console.WriteLine($"Stripe event for unknown customer {customerId} ignored");
            return null;
        }

        if (onlyExtend && user.MembershipValidUntil >= validUntil)
            return user.Id;

        user.MembershipValidUntil = validUntil;
        await db.SaveChangesAsync();
        GameHub.UpdateCachedMembership(user.Id, user.MembershipValidUntil, user.StripeCustomerId);
        return user.Id;
    }
}
