using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Components;


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
        switch (historicalEvent)
        {
            case Shipment { Passed: true }:
                return;

            case Shipment shipment:
            {
                var c = Expect<ShipmentComponent>(component);
                c.ShipmentType = shipment.ShipmentType;
                c.ShipmentForceAmount = Math.Abs(shipment.ForceAmount);
                c.ShipmentSpecialForceAmount = Math.Abs(shipment.SpecialForceAmount);
                c.ShipmentAllyContributionAmount = shipment.AllyContributionAmount;
                c.SmuggledForceAmount = shipment.SmuggledAmount;
                c.SmuggledSpecialForceAmount = shipment.SmuggledSpecialAmount;
                c.NoFieldValue = shipment.NoFieldValue;
                c.CunningNoFieldValue = shipment.CunningNoFieldValue;
                c.KarmaCard = shipment.KarmaCard;
                c._shipmentFrom = shipment.From;
                c._shipmentTo = shipment.To;
                c.ForceOrigins.Clear();
                foreach (var (location, battalion) in shipment.ForceLocations)
                {
                    c.ForceOrigins[location] = battalion;
                }
                return;
            }

            case Move move:
                ApplyPlacementValues(Expect<PlacementComponent<Move>>(component), move, move.AsAdvisors);
                return;

            case Caravan caravan:
                ApplyPlacementValues(Expect<PlacementComponent<Caravan>>(component), caravan, caravan.AsAdvisors);
                return;

            case Battle battle:
            {
                var c = Expect<BattleComponent>(component);
                c.Forces = battle.Forces + battle.ForcesAtHalfStrength;
                c.SpecialForces = battle.SpecialForces + battle.SpecialForcesAtHalfStrength;
                c.Resources = battle.Cost(battle.Game);
                c.ResourcesFromAlly = battle.AllyContributionAmount;
                c.Hero = battle.Hero;
                c.Messiah = battle.Messiah;
                c.Defense = battle.Defense;
                c.Weapon = battle.Weapon;
                c.BankerBonus = battle.BankerBonus;
                return;
            }

            case BattleConcluded battleConcluded:
            {
                var c = Expect<BattleConcludedComponent>(component);
                c.captureDecision = battleConcluded.DecisionToCapture;
                c.replacementAmount = battleConcluded.SpecialForceLossesReplaced;
                c.stolenToken = battleConcluded.StolenToken;
                c.selectedNewTraitor = battleConcluded.NewTraitor;
                c.traitorToReplace = battleConcluded.TraitorToReplace;
                c.addExtraForce = battleConcluded.AddExtraForce;
                var cards = battleConcluded.DiscardedCards.ToHashSet();
                c.discardMercenary = c.DiscardableMercenaryAfterBattle is { } mercenary && cards.Contains(mercenary);
                c.discardWeapon = c.DiscardableWeaponAfterBattle is { } weapon && cards.Contains(weapon);
                c.discardDefense = c.DiscardableDefenseAfterBattle is { } defense && cards.Contains(defense);
                return;
            }

            case DealOffered deal:
            {
                var c = Expect<DealOfferedComponent>(component);
                c.to = deal.To.ToList();
                c.price = deal.Price;
                c.benefit = deal.Benefit;
                c.type = deal.Type;
                c.text = deal.Text;
                c.until = deal.EndPhase;
                return;
            }

            case Donated donated:
            {
                var c = Expect<DonatedComponent>(component);
                c.target = donated.Target;
                c.resources = donated.Resources;
                c.card = donated.Card;
                c.fromBank = donated.FromBank;
                return;
            }

            case Revival revival:
            {
                var c = Expect<RevivalComponent>(component);
                c.amountOfForces = revival.AmountOfForces;
                c.amountOfSpecialForces = revival.AmountOfSpecialForces;
                c.forcesPaidByRed = revival.ExtraForcesPaidByRed;
                c.specialForcesPaidByRed = revival.ExtraSpecialForcesPaidByRed;
                c.hero = revival.Hero;
                c.assignSkill = revival.AssignSkill;
                c.useRedSecretAlly = revival.UsesRedSecretAlly;
                c.amountOfForcesToLocation = revival.NumberOfForcesInLocation;
                c.amountOfSpecialForcesToLocation = revival.NumberOfSpecialForcesInLocation;
                c.location = revival.Location;
                return;
            }

            case BattleInitiated battleInitiated:
            {
                var c = Expect<BattleInitiatedComponent>(component);
                c.Opponent = battleInitiated.Target;
                c.Territory = battleInitiated.Territory;
                return;
            }

            case RaiseDeadPlayed raiseDead:
            {
                var c = Expect<RaiseDeadComponent>(component);
                c.amountOfForces = raiseDead.AmountOfForces;
                c.amountOfSpecialForces = raiseDead.AmountOfSpecialForces;
                c.hero = raiseDead.Hero;
                c.assignSkill = raiseDead.AssignSkill;
                c.amountOfSpecialForcesToLocation = raiseDead.NumberOfSpecialForcesInLocation;
                c.location = raiseDead.Location;
                return;
            }

            case ClairVoyancePlayed clairvoyance:
            {
                var c = Expect<ClairVoyancePlayedComponent>(component);
                c.target = clairvoyance.Target;
                c.question = clairvoyance.Question;
                c.questionParameterA = clairvoyance.Question switch
                {
                    ClairvoyanceQuestion.LeaderAsFacedancer or
                        ClairvoyanceQuestion.LeaderAsTraitor or
                        ClairvoyanceQuestion.LeaderInBattle or
                        ClairvoyanceQuestion.WillAttackX
                        when int.TryParse(clairvoyance.QuestionParameter1, out var id) => id,
                    _ => clairvoyance.Parameter1
                };
                c.questionParameterB =
                    int.TryParse(clairvoyance.QuestionParameter2, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out var parameterB)
                        ? parameterB
                        : clairvoyance.Parameter2;
                return;
            }

            case FaceDanced faceDanced:
            {
                var c = Expect<FaceDancedComponent>(component);
                c.forces = faceDanced.ForceLocations;
                c.targetForces = faceDanced.TargetForceLocations;
                c.forcesFromReserve = faceDanced.ForcesFromReserve;
                return;
            }

            case SetIncreasedRevivalLimits revivalLimits:
                Expect<SetIncreasedRevivalLimitsComponent>(component).factions = revivalLimits.Factions.ToList();
                return;

            case AcceptOrCancelPurpleRevival purpleRevival:
            {
                var c = Expect<AcceptOrCancelPurpleRevivalComponent>(component);
                c._offerHero = purpleRevival.Hero;
                c._price = purpleRevival.Price;
                return;
            }

            case AutomationConfigured automation:
            {
                var c = Expect<AutomationConfiguredComponent>(component);
                c.NewRuleType = automation.RuleType;
                c.NewRuleBidAmount = automation.BiddingAboveAmount;
                c.NewRuleBidFaction = automation.BiddingWinningFaction;
                return;
            }

            case SetShipmentPermission shipmentPermission:
            {
                var c = Expect<SetShipmentPermissionComponent>(component);
                c.targets = shipmentPermission.Factions.ToList();
                c._crossPermission = shipmentPermission.Permission.HasFlag(ShipmentPermission.Cross);
                c._homeworldPermission = shipmentPermission.Permission.HasFlag(ShipmentPermission.ToHomeworld);
                c._discountPermission = shipmentPermission.Permission.HasFlag(ShipmentPermission.OrangeRate);
                return;
            }

            case NexusPlayed nexus:
            {
                var c = Expect<NexusPlayedComponent>(component);
                c.greenPrescienceAspect = nexus.GreenPrescienceAspect;
                c.purpleAmountOfForces = nexus.PurpleForces;
                c.purpleAmountOfSpecialForces = nexus.PurpleSpecialForces;
                c.purpleHero = nexus.PurpleHero;
                c.purpleAssignSkill = nexus.PurpleAssignSkill;
                c.purpleAmountOfSpecialForcesToLocation = nexus.PurpleNumberOfSpecialForcesInLocation;
                c.brownCard = nexus.BrownCard;
                c.pinkTerritory = nexus.PinkTerritory;
                c.pinkFaction = nexus.PinkFaction;
                c.cyanTerritory = nexus.CyanTerritory;
                c.purpleOrYellowLocation = nexus.PurpleLocation;
                return;
            }

            case KarmaHandSwap handSwap:
                Expect<KarmaHandSwapComponent>(component).selectedCards = handSwap.ReturnedCards.ToList();
                return;

            case WhiteSpecifiesAuction auction:
            {
                var c = Expect<WhiteSpecifiesAuctionComponent>(component);
                c.card = auction.Card;
                c.auctionType = auction.AuctionType;
                c.direction = auction.Direction;
                return;
            }

            case WhiteAnnouncesBlackMarket blackMarket:
            {
                var c = Expect<WhiteAnnouncesBlackMarketComponent>(component);
                c.card = blackMarket.Card;
                c.auctionType = blackMarket.AuctionType;
                c.direction = blackMarket.Direction;
                return;
            }

            case YellowSentMonster sentMonster:
                Expect<YellowSentMonsterComponent>(component).target = sentMonster.Territory;
                return;

            case KarmaMonster karmaMonster:
                Expect<KarmaMonsterComponent>(component).target = karmaMonster.Territory;
                return;

            case TraitorDiscarded traitorDiscarded:
                Expect<TraitorDiscardedComponent>(component).hero = traitorDiscarded.Traitor;
                return;

            case BlueBattleAnnouncement battleAnnouncement:
                Expect<BlueBattleAnnouncementComponent>(component).target = battleAnnouncement.Territory;
                return;

            case BlueAccompanies accompanies:
            {
                var c = Expect<BlueAccompaniesComponent>(component);
                c.target = accompanies.Location;
                c.addExtraAdvisor = accompanies.ExtraAdvisor;
                return;
            }

            case LoserConcluded loser:
            {
                var c = Expect<LoserConcludedComponent>(component);
                c._cardToKeep = loser.KeptCard;
                c._assassinate = loser.Assassinate;
                c._karmaDecision = loser.KarmaForcedKeptCardDecision;
                c._cardsToForceKeepOrDiscard = loser.ForcedKeptOrDiscardedCards.ToList();
                return;
            }

            case Retreat retreat:
            {
                var c = Expect<RetreatComponent>(component);
                c.target = retreat.Location;
                c.forces = retreat.Forces;
                c.specialForces = retreat.SpecialForces;
                return;
            }

            case CardTraded cardTrade:
            {
                var c = Expect<CardTradedComponent>(component);
                c.card = cardTrade.Card;
                c.returnCard = cardTrade.RequestedCard;
                return;
            }

            case AllyPermission allyPermission:
            {
                var c = Expect<AllyPermissionComponent>(component);
                c._spice = allyPermission.PermittedResources;
                c._karmaCard = allyPermission.PermittedKarmaCard;
                c._emperorWillPayForExtraRevival = allyPermission.RedWillPayForExtraRevival;
                c._fremenWillProtectFromShaiHulud = allyPermission.YellowWillProtectFromMonster;
                c._fremenAllowsThreeFreeRevivals = allyPermission.YellowAllowsThreeFreeRevivals;
                c._fremenShareStormPrescience = allyPermission.YellowSharesPrescience;
                c._fremenRefundsDial = allyPermission.YellowRefundsBattleDial;
                c._greenSharePrescience = allyPermission.GreenSharesPrescience;
                c._blueAllyMayUseVoice = allyPermission.BlueAllowsUseOfVoice;
                c._whiteAllyMayUseNoField = allyPermission.WhiteAllowsUseOfNoField;
                c._guildAllyMayShipAsGuild = allyPermission.OrangeAllowsShippingDiscount;
                c._purpleAllyMyReviveAsPurple = allyPermission.PurpleAllowsRevivalDiscount;
                c._greyAllyMayReplace = allyPermission.GreyAllowsReplacingCards;
                c._cyanAllyMayKeepCards = allyPermission.CyanAllowsKeepingCards;
                c._pinkAllyMayUseAmbassadors = allyPermission.PinkSharesAmbassadors;
                c._edited = true;
                return;
            }

            case AmbassadorActivated ambassadorActivated:
            {
                var c = Expect<AmbassadorActivatedComponent>(component);
                var ambassador = AmbassadorActivated.GetAmbassador(ambassadorActivated.Game);
                c._actualAmbassador = ambassador == Ambassador.Blue
                    ? ambassadorActivated.BlueSelectedAmbassador
                    : ambassador;
                c._brownCards = ambassadorActivated.BrownCards.ToList();
                c._pinkOfferAlliance = ambassadorActivated.PinkOfferAlliance;
                c._pinkGiveVidal = ambassadorActivated.PinkGiveVidalToAlly;
                c._pinkTakeVidal = ambassadorActivated.PinkTakeVidal;
                c._yellowFromTerritory = ambassadorActivated.YellowForceLocations.Keys.FirstOrDefault()?.Territory;
                c._yellowOrOrangeToLocation = ambassadorActivated.YellowOrOrangeTo;
                c._yellowForces = ambassadorActivated.YellowForceLocations;
                c._greyCard = ambassadorActivated.GreyCard;
                c._orangeForceAmount = ambassadorActivated.OrangeForceAmount;
                c._purpleAmountOfForces = ambassadorActivated.PurpleAmountOfForces;
                c._purpleHero = ambassadorActivated.PurpleHero;
                c._purpleAssignSkill = ambassadorActivated.PurpleAssignSkill;
                return;
            }

            case PlacementEvent placement:
                switch (component)
                {
                    case PlacementComponent<YellowRidesMonster> c:
                        c.forces = placement.ForceLocations;
                        break;
                    case DiscoveryEnteredComponent c:
                        c.forces = placement.ForceLocations;
                        break;
                    case PerformSetupComponent c:
                        c.forces = placement.ForceLocations;
                        break;
                    case PerformYellowSetupComponent c:
                        c.forces = placement.ForceLocations;
                        break;
                }
                break;
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

    private static TComponent Expect<TComponent>(IComponent component) where TComponent : class =>
        component as TComponent
        ?? throw new InvalidOperationException(
            $"Expected a {typeof(TComponent).Name}, but {component.GetType().Name} was rendered.");

    private static void ApplyPlacementValues<TEvent>(
        PlacementComponent<TEvent> component,
        PlacementEvent placement,
        bool asAdvisors) where TEvent : PlacementEvent, new()
    {
        component.forces = placement.ForceLocations;
        component.fromTerritory = placement.ForceLocations.Keys.FirstOrDefault()?.Territory;
        component.toLocation = placement.To;
        component.asAdvisors = asAdvisors;
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
