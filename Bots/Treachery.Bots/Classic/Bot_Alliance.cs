/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Bots;

using Shared.Model;

public partial class ClassicBot
{
    protected virtual AllianceByTerror DetermineAllianceByTerror()
    {
        var cyan = Game.GetPlayer(Faction.Cyan);
        var accept = !Player.HasAlly || PlayerStanding(cyan) > 0.75f * PlayerStanding(AlliedPlayer!);
        return new AllianceByTerror(Game, Faction) { Passed = !accept };
    }

    protected virtual AllianceByAmbassador DetermineAllianceByAmbassador()
    {
        var pink = Game.GetPlayer(Faction.Pink);
        var accept = !Player.HasAlly || PlayerStanding(pink) > 0.75f * PlayerStanding(AlliedPlayer!);
        return new AllianceByAmbassador(Game, Faction) { Passed = !accept };
    }

    private NexusVoted DetermineNexusVoted()
    {
        return new NexusVoted(Game, Faction) { Passed = false };
    }

    protected virtual AllianceOffered? DetermineAllianceOffered()
    {
        var nrOfPlayers = Game.Players.Count;
        var nrOfUnAlliedBots = Game.Players.Count(p => Game.IsBot(p) && !p.HasAlly);
        var nrOfUnalliedHumans = Game.Players.Count(p => !Game.IsBot(p) && !p.HasAlly);
        var offer = Game.CurrentAllianceOffers
                        .Where(offer => offer.Target == Faction && !Game.IsBot(offer.Player))
                        .HighestOrDefault(offer => PlayerStanding(offer.Player))
                    ?? Game.CurrentAllianceOffers
                        .Where(allianceOffered => allianceOffered.Target == Faction)
                        .HighestOrDefault(allianceOffered => PlayerStanding(allianceOffered.Player));

        if (offer != null) 
            return new AllianceOffered(Game, Faction) { Target = offer.Initiator };

        if (nrOfPlayers <= 2 ||
            Game.Applicable(Rule.BotsCannotAlly) ||
            (nrOfUnalliedHumans != 0 && nrOfUnalliedHumans >= nrOfUnAlliedBots - 1) ||
            Game.CurrentAllianceOffers.Any(o =>
                o.Initiator == Faction && Game.GetPlayer(o.Target).Ally == Faction.None)) return null;
        {
            var mostInterestingOpponentBotWithoutAlly = Game.Players
                .Where(p => p != Player && Game.IsBot(p) && !p.HasAlly)
                .HighestOrDefault(p => PlayerStanding(p));

            if (mostInterestingOpponentBotWithoutAlly != null) 
                return new AllianceOffered(Game, Faction) { Target = mostInterestingOpponentBotWithoutAlly.Faction };
        }

        return null;
    }

    private int PlayerStanding(Player p)
    {
        return 2 * p.TreacheryCards.Count() +
               p.Resources +
               6 * p.LocationsWithAnyForces.Count(l => l.Territory.IsStronghold) +
               p.ForcesOnPlanet.Sum(b => b.Value.TotalAmountOfForces) +
               p.ForcesInReserve +
               2 * p.SpecialForcesInReserve +
               2 * p.Leaders.Count(l => Game.IsAlive(l));
    }

    protected virtual AllianceBroken? DetermineAllianceBroken()
    {
        var offer = Game.CurrentAllianceOffers
            .Where(offer => offer.Target == Faction && !Game.IsBot(offer.Player))
            .HighestOrDefault(offer => PlayerStanding(offer.Player));

        if (offer != null && Player.HasAlly && PlayerStanding(offer.Player) > PlayerStanding(AlliedPlayer!)) 
            return new AllianceBroken(Game, Faction);

        return null;
    }

    protected virtual NexusCardDrawn DetermineNexusCardDrawn()
    {
        if (NexusCardDrawn.MayDraw(Game, Player))
            if (Player.Nexus == Faction.None ||
                (Faction == Faction.Red && Player.Nexus == Faction.Red && !Game.Applicable(Rule.RedSpecialForces)) ||
                (Faction == Faction.Blue && Player.Nexus == Faction.Blue && !Game.Applicable(Rule.BlueAdvisors)) ||
                (Faction == Faction.Grey && Player.Nexus == Faction.Grey && !Game.Applicable(Rule.AdvancedCombat)))
                return new NexusCardDrawn(Game, Faction) { Passed = false };

        return new NexusCardDrawn(Game, Faction) { Passed = true };
    }

    protected virtual AllyPermission? DetermineAlliancePermissions()
    {
        if (Ally == Faction.None) return null;

        if (Game.CurrentMainPhase != MainPhase.Blow && Game.CurrentMainPhase != MainPhase.Bidding &&
            Game.CurrentMainPhase != MainPhase.ShipmentAndMove &&
            Game.CurrentMainPhase != MainPhase.Battle) return null;
        
        var allowedResources = DetermineAllowedResources();
        var allowedKarmaCard = Player.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);
        var permission = new AllyPermission(Game, Faction) { PermittedResources = allowedResources, PermittedKarmaCard = allowedKarmaCard };
        var boolPermissionsNeedUpdate = BoolPermissionsNeedUpdate(permission);
        var specialPermissionsNeedUpdate = SpecialPermissionsNeedUpdate(permission);

        if (!boolPermissionsNeedUpdate && !specialPermissionsNeedUpdate &&
            Game.GetPermittedUseOfAllyResources(Ally) == allowedResources &&
            Game.GetPermittedUseOfAllyKarma(Ally) == allowedKarmaCard) return null;
        
        LogInfo("Updating permissions, allowing use of {0} spice and Karama: {1}", allowedResources, allowedKarmaCard);
        return permission;
    }

    private bool SpecialPermissionsNeedUpdate(AllyPermission permission)
    {
        var result = false;

        switch (Faction)
        {
            case Faction.Red:
                permission.RedWillPayForExtraRevival = 3;
                if (Game.RedWillPayForExtraRevival != 3) result = true;
                break;
        }

        return result;
    }

    private int DetermineAllowedResources()
    {
        switch (Faction)
        {
            case Faction.Green:
            case Faction.Blue:
            case Faction.White:
            case Faction.Black:
            case Faction.Pink:
            case Faction.Cyan:
            case Faction.Grey:

                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                    return Math.Max(Resources - 5, 0);
                return Math.Max(Resources - 10, 0);

            case Faction.Brown:
                if (Game.CurrentMainPhase == MainPhase.Battle)
                    return Resources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                    return Math.Max(Resources - 5, 0);
                return Math.Max(Resources - 10, 0);

            case Faction.Orange:
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
                    return Resources;
                return Math.Max(Resources - 10, 0);

            case Faction.Yellow:
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove || Player.TreacheryCards.Count() == 4)
                    return Resources;
                return Math.Max(Resources - 10, 0);

            case Faction.Red:
                if (Game.CurrentMainPhase == MainPhase.Bidding)
                    return Resources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                    return Math.Max(Resources - 5, 0);
                return Math.Max(Resources - 10, 0);

            case Faction.Purple:
                if (Game.CurrentMainPhase == MainPhase.Resurrection)
                    return Resources;
                if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.HasActedOrPassed.Contains(Faction))
                    return Math.Max(Resources - 5, 0);
                return Math.Max(Resources - 10, 0);
        }

        return 0;
    }

    private bool BoolPermissionsNeedUpdate(AllyPermission permission)
    {
        var result = false;

        foreach (var p in GetPermissionProperties())
        {
            var prop = typeof(AllyPermission).GetProperty(p);
            if (prop == null) continue;
            prop.SetValue(permission, true);
            
            if (result) continue;
            
            var gameProp = typeof(Game).GetProperty(p);
            if (Equals(gameProp?.GetValue(Game), true))
                result = true;
        }

        return result;
    }

    private IEnumerable<string> GetPermissionProperties() => Faction switch
    {
        Faction.Green => ["GreenSharesPrescience"],
        Faction.Yellow => ["YellowSharesPrescience", "YellowWillProtectFromMonster", "YellowAllowsThreeFreeRevivals", "YellowRefundsBattleDial"],
        Faction.Orange => ["OrangeAllowsShippingDiscount"],
        Faction.Blue => ["BlueAllowsUseOfVoice"],
        Faction.Grey => ["GreyAllowsReplacingCards"],
        Faction.Purple => ["PurpleAllowsRevivalDiscount"],
        Faction.White => ["WhiteAllowsUseOfNoField"],
        Faction.Cyan => ["CyanAllowsKeepingCards"],
        Faction.Pink => ["PinkSharesAmbassadors"],
        _ => []
    };

    protected virtual CardTraded? DetermineCardTraded()
    {
        if (Game.CurrentCardTradeOffer == null) return null;

        return Game.CurrentCardTradeOffer.RequestedCard != null 
            ? new CardTraded(Game, Faction) { Target = Game.CurrentCardTradeOffer.Initiator, Card = Game.CurrentCardTradeOffer.RequestedCard, RequestedCard = null } 
            : new CardTraded(Game, Faction) { Target = Game.CurrentCardTradeOffer.Initiator, Card = Player.TreacheryCards.OrderBy(c => CardQuality(c, Player)).FirstOrDefault(), RequestedCard = null };
    }
}