using Stripe;

namespace Treachery.Server;

public partial class GameHub
{
    private bool MembershipRequired => bool.TryParse(Configuration["MembershipRequired"], out var required) && required;

    private bool IsAdmin(LoggedInUser user) => user.Username == Configuration["GameAdminUsername"];

    private bool CanUseAdvancedBots(LoggedInUser user) =>
        !MembershipRequired || IsAdmin(user) || Membership.IsActive(user.User.MembershipValidUntil);

    private bool RequiresMembershipFor(LoggedInUser user, Game game) =>
        game.Applicable(Rule.AdvancedBots) && !CanUseAdvancedBots(user);

    /// <summary>
    /// Keeps the in-memory copies of a user in sync after their membership changed in the database.
    /// </summary>
    internal static void UpdateCachedMembership(int userId, DateTimeOffset? validUntil, string? stripeCustomerId)
    {
        foreach (var loggedInUser in UsersByUserToken.Values.Where(u => u.Id == userId))
        {
            loggedInUser.User.MembershipValidUntil = validUntil;
            loggedInUser.User.StripeCustomerId = stripeCustomerId;
        }

        if (UsersById.TryGetValue(userId, out var user))
        {
            user.MembershipValidUntil = validUntil;
            user.StripeCustomerId = stripeCustomerId;
        }
    }

    public async Task<Result<string>> RequestMembershipCheckout(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var loggedInUser))
            return Error<string>(ErrorType.UserNotFound);

        var priceId = Configuration["StripePriceId"];
        if (StripeClientOrNull() is not { } stripe || string.IsNullOrEmpty(priceId))
            return Error<string>(ErrorType.MembershipUnavailable);

        try
        {
            var user = loggedInUser.User;
            var customerId = user.StripeCustomerId;
            if (string.IsNullOrEmpty(customerId))
            {
                var customer = await stripe.V1.Customers.CreateAsync(new CustomerCreateOptions
                {
                    Email = user.Email,
                    Name = user.PlayerName,
                    Metadata = new Dictionary<string, string> { ["userId"] = user.Id.ToString() }
                });
                customerId = customer.Id;

                await using var db = GetDbContext();
                await db.Users.Where(u => u.Id == user.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.StripeCustomerId, customerId));
                UpdateCachedMembership(user.Id, user.MembershipValidUntil, customerId);
            }

            var returnUrl = SiteUrl();
            var session = await stripe.V1.Checkout.Sessions.CreateAsync(new Stripe.Checkout.SessionCreateOptions
            {
                Mode = "subscription",
                Customer = customerId,
                ClientReferenceId = user.Id.ToString(),
                LineItems = [new Stripe.Checkout.SessionLineItemOptions { Price = priceId, Quantity = 1 }],
                SubscriptionData = new Stripe.Checkout.SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string> { ["userId"] = user.Id.ToString() }
                },
                SuccessUrl = returnUrl,
                CancelUrl = returnUrl
            });

            return Success(session.Url);
        }
        catch (StripeException e)
        {
            Log($"{nameof(RequestMembershipCheckout)} failed: {e.Message}");
            return Error<string>(ErrorType.MembershipUnavailable);
        }
    }

    public async Task<Result<string>> RequestMembershipPortal(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var loggedInUser))
            return Error<string>(ErrorType.UserNotFound);

        var customerId = loggedInUser.User.StripeCustomerId;
        if (string.IsNullOrEmpty(customerId))
            return Error<string>(ErrorType.MembershipNotFound);

        if (StripeClientOrNull() is not { } stripe)
            return Error<string>(ErrorType.MembershipUnavailable);

        try
        {
            var session = await stripe.V1.BillingPortal.Sessions.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = SiteUrl()
            });

            return Success(session.Url);
        }
        catch (StripeException e)
        {
            Log($"{nameof(RequestMembershipPortal)} failed: {e.Message}");
            return Error<string>(ErrorType.MembershipUnavailable);
        }
    }

    private StripeClient? StripeClientOrNull()
    {
        var secretKey = Configuration["StripeSecretKey"];
        return string.IsNullOrEmpty(secretKey) ? null : new StripeClient(secretKey);
    }

    private string SiteUrl()
    {
        var configured = Configuration["SiteUrl"];
        if (!string.IsNullOrEmpty(configured))
            return configured;

        var request = Context.GetHttpContext()?.Request;
        return request != null ? $"{request.Scheme}://{request.Host}/" : "https://treachery.online/";
    }
}
