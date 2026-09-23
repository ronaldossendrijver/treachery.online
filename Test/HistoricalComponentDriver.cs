using System.Globalization;
using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using Treachery.Client.GameEventComponents;
using Treachery.Client.GenericComponents;
using Treachery.Shared;

namespace Treachery.Test;

internal static class HistoricalComponentDriver
{
    private static readonly Assembly ClientAssembly = typeof(GameEventComponent<>).Assembly;

    private static readonly IReadOnlyDictionary<string, string> MemberAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["amount"] = "amount",
            ["faction"] = "_faction",
            ["traitor"] = "traitor",
            ["selectedtraitor"] = "traitor",
            ["permittedresources"] = "spice",
            ["redwillpayforextrarevival"] = "emperorWillPayForExtraRevival",
            ["yellowwillprotectfrommonster"] = "fremenWillProtectFromShaiHulud",
            ["yellowallowsthreefreerevivals"] = "fremenAllowsThreeFreeRevivals",
            ["yellowsharesprescience"] = "fremenShareStormPrescience",
            ["yellowrefundsbattledial"] = "fremenRefundsDial",
            ["greensharesprescience"] = "greenSharePrescience",
            ["blueallowsuseofvoice"] = "blueAllyMayUseVoice",
            ["whiteallowsuseofnofield"] = "whiteAllyMayUseNoField",
            ["orangeallowsshippingdiscount"] = "guildAllyMayShipAsGuild",
            ["purpleallowsrevivaldiscount"] = "purpleAllyMyReviveAsPurple",
            ["greyallowsreplacingcards"] = "greyAllyMayReplace",
            ["cyanallowskeepingcards"] = "cyanAllyMayKeepCards",
            ["pinksharesambassadors"] = "pinkAllyMayUseAmbassadors",
            ["parameter1"] = "questionParameterA",
            ["parameter2"] = "questionParameterB",
            ["moveamount"] = "nrOfSectors",
            ["specialforcelossesreplaced"] = "replacementAmount",
            ["valueadded"] = "amount",
            ["hero"] = "hero"
        };

    public static IRenderedComponent<DynamicComponent> Render(
        ComponentTestContext test,
        GameEvent historicalEvent)
    {
        var componentType = ResolveComponentType(historicalEvent);
        var componentParameters = historicalEvent is EstablishPlayers establishPlayers
            ? new Dictionary<string, object>
            {
                [nameof(EstablishPlayersComponent.SeedFactory)] =
                    (Func<int>)(() => establishPlayers.Seed)
            }
            : null;
        var rendered = test.RenderContext.Render<DynamicComponent>(parameters =>
        {
            parameters.Add(component => component.Type, componentType);
            if (componentParameters != null)
            {
                parameters.Add(component => component.Parameters, componentParameters);
            }
        });
        if (rendered.Instance.Instance is not IComponent component)
        {
            throw new InvalidOperationException($"Could not instantiate {componentType.Name}.");
        }

        ApplyHistoricalValues(component, historicalEvent);
        RefreshComponent(rendered, component);
        if (rendered.Instance.Instance is not IComponent renderedComponent)
        {
            throw new InvalidOperationException($"Could not re-render {componentType.Name}.");
        }

        ApplyHistoricalValues(renderedComponent, historicalEvent);
        RefreshComponent(rendered, renderedComponent);
        return rendered;
    }

    public static HistoricalComponentResult ReadResult(
        IComponent component,
        GameEvent historicalEvent)
    {
        var preferredProperty = historicalEvent is PassableGameEvent { Passed: true }
            ? "PassedResult"
            : "ConfirmedResult";
        var candidates = new[]
            {
                preferredProperty, "ConfirmedResult", "PassedResult", "OtherResult"
            }
            .Distinct()
            .Select(name => new HistoricalComponentResult(
                name,
                FindResultProperty(component.GetType(), name)?.GetValue(component) as GameEvent))
            .Where(candidate => candidate.GameEvent != null)
            .ToArray();

        return candidates.FirstOrDefault(candidate =>
                   HistoricalGameEventComparer.AreEquivalent(
                       historicalEvent,
                       candidate.GameEvent!))
               ?? candidates.FirstOrDefault()
               ?? throw new InvalidOperationException(
                   $"{component.GetType().Name} did not produce a GameEvent result.");
    }

    public static void AssertInputsAreRepresentable(IRenderedComponent<DynamicComponent> rendered)
    {
        foreach (var select in rendered.FindAll("select"))
        {
            var selected = select.QuerySelectorAll("option[selected]");
            Assert.IsNotEmpty(
                selected,
                $"Select input in {rendered.Instance.Instance?.GetType().Name} does not represent an available value.");
        }

    }

    private static Type ResolveComponentType(GameEvent historicalEvent)
    {
        var eventType = historicalEvent.GetType();
        var concreteName = eventType.Name switch
        {
            nameof(YellowRidesMonster) => nameof(YellowRideComponent),
            nameof(RaiseDeadPlayed) => nameof(RaiseDeadComponent),
            _ => $"{eventType.Name}Component"
        };

        var concrete = ClientAssembly.GetType(
            $"Treachery.Client.GameEventComponents.{concreteName}");
        if (concrete != null)
        {
            return concrete;
        }

        return historicalEvent is PassableGameEvent
            ? typeof(YesNoActionComponent<>).MakeGenericType(eventType)
            : typeof(SimpleActionComponent<>).MakeGenericType(eventType);
    }

    private static void ApplyHistoricalValues(IComponent component, GameEvent historicalEvent)
    {
        if (historicalEvent is Shipment shipment)
        {
            if (shipment.Passed)
            {
                return;
            }

            SetMember(component, "ShipmentType", shipment.ShipmentType);
            SetMember(component, "ShipmentForceAmount", Math.Abs(shipment.ForceAmount));
            SetMember(component, "ShipmentSpecialForceAmount", Math.Abs(shipment.SpecialForceAmount));
            SetMember(component, "ShipmentAllyContributionAmount", shipment.AllyContributionAmount);
            SetMember(component, "SmuggledForceAmount", shipment.SmuggledAmount);
            SetMember(component, "SmuggledSpecialForceAmount", shipment.SmuggledSpecialAmount);
            SetMember(component, "NoFieldValue", shipment.NoFieldValue);
            SetMember(component, "CunningNoFieldValue", shipment.CunningNoFieldValue);
            SetMember(component, "KarmaCard", shipment.KarmaCard);
            SetMember(component, "_shipmentFrom", shipment.From);
            SetMember(component, "_shipmentTo", shipment.To);
            if (GetMemberValue<Dictionary<Location, Battalion>>(
                    component,
                    "ForceOrigins") is { } forceOrigins)
            {
                forceOrigins.Clear();
                foreach (var (location, battalion) in shipment.ForceLocations)
                {
                    forceOrigins[location] = battalion;
                }
            }
            return;
        }

        if (historicalEvent is Move move)
        {
            ApplyPlacementValues(component, move, move.AsAdvisors);
            return;
        }

        if (historicalEvent is Caravan caravan)
        {
            ApplyPlacementValues(component, caravan, caravan.AsAdvisors);
            return;
        }

        if (historicalEvent is Battle battle)
        {
            SetMember(component, "Forces", battle.Forces + battle.ForcesAtHalfStrength);
            SetMember(component, "SpecialForces",
                battle.SpecialForces + battle.SpecialForcesAtHalfStrength);
            SetMember(component, "Resources", battle.Cost(battle.Game));
            SetMember(component, "ResourcesFromAlly", battle.AllyContributionAmount);
            SetMember(component, "Hero", battle.Hero);
            SetMember(component, "Messiah", battle.Messiah);
            SetMember(component, "Defense", battle.Defense);
            SetMember(component, "Weapon", battle.Weapon);
            SetMember(component, "BankerBonus", battle.BankerBonus);
            return;
        }

        if (historicalEvent is BattleConcluded battleConcluded)
        {
            SetMember(component, "captureDecision", battleConcluded.DecisionToCapture);
            SetMember(component, "replacementAmount",
                battleConcluded.SpecialForceLossesReplaced);
            SetMember(component, "stolenToken", battleConcluded.StolenToken);
            SetMember(component, "selectedNewTraitor", battleConcluded.NewTraitor);
            SetMember(component, "traitorToReplace", battleConcluded.TraitorToReplace);
            SetMember(component, "addExtraForce", battleConcluded.AddExtraForce);
            var cards = battleConcluded.DiscardedCards.ToHashSet();
            SetMember(component, "discardMercenary",
                GetMemberValue<TreacheryCard>(component, "DiscardableMercenaryAfterBattle") is { } mercenary &&
                cards.Contains(mercenary));
            SetMember(component, "discardWeapon",
                GetMemberValue<TreacheryCard>(component, "DiscardableWeaponAfterBattle") is { } weapon &&
                cards.Contains(weapon));
            SetMember(component, "discardDefense",
                GetMemberValue<TreacheryCard>(component, "DiscardableDefenseAfterBattle") is { } defense &&
                cards.Contains(defense));
            return;
        }

        if (historicalEvent is DealOffered deal)
        {
            SetMember(component, "to", deal.To.ToList());
            SetMember(component, "price", deal.Price);
            SetMember(component, "benefit", deal.Benefit);
            SetMember(component, "type", deal.Type);
            SetMember(component, "text", deal.Text);
            SetMember(component, "until", deal.EndPhase);
            return;
        }

        if (historicalEvent is Donated donated)
        {
            SetMember(component, "target", donated.Target);
            SetMember(component, "resources", donated.Resources);
            SetMember(component, "card", donated.Card);
            SetMember(component, "fromBank", donated.FromBank);
            return;
        }

        if (historicalEvent is Revival revival)
        {
            SetMember(component, "amountOfForces", revival.AmountOfForces);
            SetMember(component, "amountOfSpecialForces", revival.AmountOfSpecialForces);
            SetMember(component, "forcesPaidByRed", revival.ExtraForcesPaidByRed);
            SetMember(component, "specialForcesPaidByRed",
                revival.ExtraSpecialForcesPaidByRed);
            SetMember(component, "hero", revival.Hero);
            SetMember(component, "assignSkill", revival.AssignSkill);
            SetMember(component, "useRedSecretAlly", revival.UsesRedSecretAlly);
            SetMember(component, "amountOfForcesToLocation",
                revival.NumberOfForcesInLocation);
            SetMember(component, "amountOfSpecialForcesToLocation",
                revival.NumberOfSpecialForcesInLocation);
            SetMember(component, "location", revival.Location);
            return;
        }

        if (historicalEvent is BattleInitiated battleInitiated)
        {
            SetMember(component, "Opponent", battleInitiated.Target);
            SetMember(component, "Territory", battleInitiated.Territory);
            return;
        }

        if (historicalEvent is RaiseDeadPlayed raiseDead)
        {
            SetMember(component, "amountOfForces", raiseDead.AmountOfForces);
            SetMember(component, "amountOfSpecialForces",
                raiseDead.AmountOfSpecialForces);
            SetMember(component, "hero", raiseDead.Hero);
            SetMember(component, "assignSkill", raiseDead.AssignSkill);
            SetMember(component, "amountOfSpecialForcesToLocation",
                raiseDead.NumberOfSpecialForcesInLocation);
            SetMember(component, "location", raiseDead.Location);
            return;
        }

        if (historicalEvent is ClairVoyancePlayed clairvoyance)
        {
            SetMember(component, "target", clairvoyance.Target);
            SetMember(component, "question", clairvoyance.Question);
            object? parameterA = clairvoyance.Question switch
            {
                ClairvoyanceQuestion.LeaderAsFacedancer or
                    ClairvoyanceQuestion.LeaderAsTraitor or
                    ClairvoyanceQuestion.LeaderInBattle or
                    ClairvoyanceQuestion.WillAttackX
                    when int.TryParse(clairvoyance.QuestionParameter1, out var id) => id,
                _ => clairvoyance.Parameter1
            };
            SetMember(component, "questionParameterA", parameterA);
            SetMember(component, "questionParameterB",
                int.TryParse(clairvoyance.QuestionParameter2, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var parameterB)
                    ? parameterB
                    : clairvoyance.Parameter2);
            return;
        }

        if (historicalEvent is FaceDanced faceDanced)
        {
            SetMember(component, "forces", faceDanced.ForceLocations);
            SetMember(component, "targetForces", faceDanced.TargetForceLocations);
            SetMember(component, "forcesFromReserve",
                faceDanced.ForcesFromReserve);
            return;
        }

        if (historicalEvent is SetIncreasedRevivalLimits revivalLimits)
        {
            SetMember(component, "factions", revivalLimits.Factions.ToList());
            return;
        }

        if (historicalEvent is AcceptOrCancelPurpleRevival purpleRevival)
        {
            SetMember(component, "offerHero", purpleRevival.Hero);
            SetMember(component, "price", purpleRevival.Price);
            return;
        }

        if (historicalEvent is AutomationConfigured automation)
        {
            SetMember(component, "NewRuleType", automation.RuleType);
            SetMember(component, "NewRuleBidAmount",
                automation.BiddingAboveAmount);
            SetMember(component, "NewRuleBidFaction",
                automation.BiddingWinningFaction);
            return;
        }

        if (historicalEvent is SetShipmentPermission shipmentPermission)
        {
            SetMember(component, "targets",
                shipmentPermission.Factions.ToList());
            SetMember(component, "_crossPermission",
                shipmentPermission.Permission.HasFlag(ShipmentPermission.Cross));
            SetMember(component, "_homeworldPermission",
                shipmentPermission.Permission.HasFlag(ShipmentPermission.ToHomeworld));
            SetMember(component, "_discountPermission",
                shipmentPermission.Permission.HasFlag(ShipmentPermission.OrangeRate));
            return;
        }

        if (historicalEvent is NexusPlayed nexus)
        {
            SetMember(component, "greenPrescienceAspect",
                nexus.GreenPrescienceAspect);
            SetMember(component, "purpleAmountOfForces", nexus.PurpleForces);
            SetMember(component, "purpleAmountOfSpecialForces",
                nexus.PurpleSpecialForces);
            SetMember(component, "purpleHero", nexus.PurpleHero);
            SetMember(component, "purpleAssignSkill", nexus.PurpleAssignSkill);
            SetMember(component, "purpleAmountOfSpecialForcesToLocation",
                nexus.PurpleNumberOfSpecialForcesInLocation);
            SetMember(component, "brownCard", nexus.BrownCard);
            SetMember(component, "pinkTerritory", nexus.PinkTerritory);
            SetMember(component, "pinkFaction", nexus.PinkFaction);
            SetMember(component, "cyanTerritory", nexus.CyanTerritory);
            SetMember(component, "purpleOrYellowLocation", nexus.PurpleLocation);
            return;
        }

        if (historicalEvent is KarmaHandSwap handSwap)
        {
            SetMember(component, "selectedCards",
                handSwap.ReturnedCards.ToList());
            return;
        }

        if (historicalEvent is WhiteSpecifiesAuction auction)
        {
            SetMember(component, "card", auction.Card);
            SetMember(component, "auctionType", auction.AuctionType);
            SetMember(component, "direction", auction.Direction);
            return;
        }

        if (historicalEvent is WhiteAnnouncesBlackMarket blackMarket)
        {
            SetMember(component, "card", blackMarket.Card);
            SetMember(component, "auctionType", blackMarket.AuctionType);
            SetMember(component, "direction", blackMarket.Direction);
            return;
        }

        if (historicalEvent is YellowSentMonster sentMonster)
        {
            SetMember(component, "target", sentMonster.Territory);
            return;
        }

        if (historicalEvent is KarmaMonster karmaMonster)
        {
            SetMember(component, "target", karmaMonster.Territory);
            return;
        }

        if (historicalEvent is TraitorDiscarded traitorDiscarded)
        {
            SetMember(component, "hero", traitorDiscarded.Traitor);
            return;
        }

        if (historicalEvent is BlueBattleAnnouncement battleAnnouncement)
        {
            SetMember(component, "target", battleAnnouncement.Territory);
            return;
        }

        if (historicalEvent is BlueAccompanies accompanies)
        {
            SetMember(component, "target", accompanies.Location);
            SetMember(component, "addExtraAdvisor", accompanies.ExtraAdvisor);
            return;
        }

        if (historicalEvent is LoserConcluded loser)
        {
            SetMember(component, "_cardToKeep", loser.KeptCard);
            SetMember(component, "_assassinate", loser.Assassinate);
            SetMember(component, "_karmaDecision",
                loser.KarmaForcedKeptCardDecision);
            SetMember(component, "_cardsToForceKeepOrDiscard",
                loser.ForcedKeptOrDiscardedCards.ToList());
            return;
        }

        if (historicalEvent is Retreat retreat)
        {
            SetMember(component, "target", retreat.Location);
            SetMember(component, "forces", retreat.Forces);
            SetMember(component, "specialForces", retreat.SpecialForces);
            return;
        }

        if (historicalEvent is CardTraded cardTrade)
        {
            SetMember(component, "card", cardTrade.Card);
            SetMember(component, "returnCard", cardTrade.RequestedCard);
            return;
        }

        if (historicalEvent is AllyPermission allyPermission)
        {
            SetMember(component, "spice", allyPermission.PermittedResources);
            SetMember(component, "karmaCard", allyPermission.PermittedKarmaCard);
            SetMember(component, "emperorWillPayForExtraRevival",
                allyPermission.RedWillPayForExtraRevival);
            SetMember(component, "fremenWillProtectFromShaiHulud",
                allyPermission.YellowWillProtectFromMonster);
            SetMember(component, "fremenAllowsThreeFreeRevivals",
                allyPermission.YellowAllowsThreeFreeRevivals);
            SetMember(component, "fremenShareStormPrescience",
                allyPermission.YellowSharesPrescience);
            SetMember(component, "fremenRefundsDial",
                allyPermission.YellowRefundsBattleDial);
            SetMember(component, "greenSharePrescience",
                allyPermission.GreenSharesPrescience);
            SetMember(component, "blueAllyMayUseVoice",
                allyPermission.BlueAllowsUseOfVoice);
            SetMember(component, "whiteAllyMayUseNoField",
                allyPermission.WhiteAllowsUseOfNoField);
            SetMember(component, "guildAllyMayShipAsGuild",
                allyPermission.OrangeAllowsShippingDiscount);
            SetMember(component, "purpleAllyMyReviveAsPurple",
                allyPermission.PurpleAllowsRevivalDiscount);
            SetMember(component, "greyAllyMayReplace",
                allyPermission.GreyAllowsReplacingCards);
            SetMember(component, "cyanAllyMayKeepCards",
                allyPermission.CyanAllowsKeepingCards);
            SetMember(component, "pinkAllyMayUseAmbassadors",
                allyPermission.PinkSharesAmbassadors);
            SetMember(component, "edited", true);
            return;
        }

        if (historicalEvent is AmbassadorActivated ambassadorActivated)
        {
            SetMember(component, "_actualAmbassador",
                AmbassadorActivated.GetAmbassador(ambassadorActivated.Game) == Ambassador.Blue
                    ? ambassadorActivated.BlueSelectedAmbassador
                    : AmbassadorActivated.GetAmbassador(ambassadorActivated.Game));
            SetMember(component, "_brownCards",
                ambassadorActivated.BrownCards.ToList());
            SetMember(component, "_pinkOfferAlliance",
                ambassadorActivated.PinkOfferAlliance);
            SetMember(component, "_pinkGiveVidal",
                ambassadorActivated.PinkGiveVidalToAlly);
            SetMember(component, "_pinkTakeVidal",
                ambassadorActivated.PinkTakeVidal);
            SetMember(component, "_yellowFromTerritory",
                ambassadorActivated.YellowForceLocations.Keys
                    .FirstOrDefault()?.Territory);
            SetMember(component, "_yellowOrOrangeToLocation",
                ambassadorActivated.YellowOrOrangeTo);
            SetMember(component, "_yellowForces",
                ambassadorActivated.YellowForceLocations);
            SetMember(component, "_greyCard", ambassadorActivated.GreyCard);
            SetMember(component, "_orangeForceAmount",
                ambassadorActivated.OrangeForceAmount);
            SetMember(component, "_purpleAmountOfForces",
                ambassadorActivated.PurpleAmountOfForces);
            SetMember(component, "_purpleHero", ambassadorActivated.PurpleHero);
            SetMember(component, "_purpleAssignSkill",
                ambassadorActivated.PurpleAssignSkill);
            return;
        }

        if (historicalEvent is PlacementEvent placement)
        {
            SetMember(component, "forces", placement.ForceLocations);
        }

        foreach (var source in ReadableMembers(historicalEvent.GetType()))
        {
            object? value;
            try
            {
                value = source.GetValue(historicalEvent);
            }
            catch
            {
                continue;
            }

            var targetName = MemberAliases.GetValueOrDefault(Normalize(source.Name), source.Name);
            var writableMembers = WritableMembers(component.GetType()).ToArray();
            var target = writableMembers.FirstOrDefault(member =>
                    Normalize(member.Name) == Normalize(targetName) &&
                    CanAssign(member.ValueType, value));
            var suffixMatches = writableMembers
                .Where(member =>
                    (Normalize(member.Name).Contains(Normalize(targetName)) ||
                     Normalize(targetName).Contains(Normalize(member.Name))) &&
                                 CanAssign(member.ValueType, value))
                .ToArray();
            target ??= suffixMatches.Length == 1 ? suffixMatches[0] : null;

            target?.SetValue(component, value);
        }
    }

    private static void SetMember(IComponent component, string name, object? value)
    {
        var member = WritableMembers(component.GetType())
            .FirstOrDefault(candidate => candidate.Name == name);
        member?.SetValue(component, value);
    }

    private static void ApplyPlacementValues(
        IComponent component,
        PlacementEvent placement,
        bool asAdvisors)
    {
        var forceLocations = placement.ForceLocations;
        SetMember(component, "forces", forceLocations);
        SetMember(component, "fromTerritory",
            forceLocations.Keys.FirstOrDefault()?.Territory);
        SetMember(component, "toLocation", placement.To);
        SetMember(component, "asAdvisors", asAdvisors);
    }

    private static T? GetMemberValue<T>(IComponent component, string name) where T : class
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (var current = component.GetType(); current != null; current = current.BaseType)
        {
            var property = current.GetProperty(name, flags | BindingFlags.DeclaredOnly);
            if (property?.GetValue(component) is T value)
            {
                return value;
            }
        }

        return null;
    }

    private static void RefreshComponent(
        IRenderedComponent<DynamicComponent> rendered,
        IComponent component)
    {
        var stateHasChanged = typeof(ComponentBase).GetMethod(
            "StateHasChanged",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Could not request a component render.");
        rendered.InvokeAsync(() => stateHasChanged.Invoke(component, null))
            .GetAwaiter()
            .GetResult();
    }

    private static IEnumerable<ReadableMember> ReadableMembers(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
        return type.GetProperties(flags)
                .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                .Select(property => new ReadableMember(property.Name, property.GetValue))
            .Concat(type.GetFields(flags)
                .Select(field => new ReadableMember(field.Name, field.GetValue)));
    }

    private static IEnumerable<WritableMember> WritableMembers(Type type)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        for (var current = type; current != null; current = current.BaseType)
        {
            foreach (var field in current.GetFields(flags)
                         .Where(field => !field.IsInitOnly &&
                                         !field.Name.Contains("k__BackingField", StringComparison.Ordinal)))
            {
                yield return new WritableMember(field.Name, field.FieldType, field.SetValue);
            }

            foreach (var property in current.GetProperties(flags)
                         .Where(property => property.SetMethod != null &&
                                            property.GetIndexParameters().Length == 0))
            {
                yield return new WritableMember(property.Name, property.PropertyType, property.SetValue);
            }
        }
    }

    private static PropertyInfo? FindResultProperty(Type type, string propertyName)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        for (var current = type; current != null; current = current.BaseType)
        {
            var property = current.GetProperty(propertyName, flags);
            if (property != null)
            {
                return property;
            }
        }

        return null;
    }

    private static bool CanAssign(Type targetType, object? value) =>
        value == null ? !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null
            : targetType.IsInstanceOfType(value);

    private static string Normalize(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private sealed record ReadableMember(string Name, Func<object, object?> GetValue);

    private sealed record WritableMember(
        string Name,
        Type ValueType,
        Action<object, object?> SetValue);
}

internal sealed record HistoricalComponentResult(
    string PropertyName,
    GameEvent? GameEvent);
