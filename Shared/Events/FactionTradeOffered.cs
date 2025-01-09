/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class FactionTradeOffered : GameEvent
{
    #region Construction

    public FactionTradeOffered(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public FactionTradeOffered()
    {
    }

    #endregion Construction

    #region Properties

    public Faction Target { get; set; }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        if (Game.Version >= 132)
        {
            if (!ValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid target");
        }
        else
        {
            if (!Game.IsPlaying(Target)) return Message.Express("Invalid target");
        }


        return null;
    }

    public static IEnumerable<Faction> ValidTargets(Game g, Player p)
    {
        return g.FactionsInPlay.Union(g.Players.Select(p => p.Faction)).Where(f => f != p.Faction);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if (!Game.IsPlaying(Target))
        {
            Log(Initiator, " switch to ", Target);
            
            if (Game.Version < 172)
                Game.FactionsInPlay.Add(Initiator);
            
            Player.Faction = Target;
        }
        else
        {
            var match = Game.CurrentTradeOffers.SingleOrDefault(matchingOffer => matchingOffer.Initiator == Target && matchingOffer.Target == Initiator);
            if (match != null)
            {
                Log(Initiator, " and ", match.Initiator, " traded factions");
                var target = GetPlayer(Target);
                (target.Faction, Player.Faction) = (Player.Faction, target.Faction);
                RemoveInvalidTradeOffers();
            }
            else
            {
                Log(GetMessage());
                if (!Game.CurrentTradeOffers.Any(o => o.Initiator == Initiator && o.Target == Target)) Game.CurrentTradeOffers.Add(this);
            }
        }
    }

    private void RemoveInvalidTradeOffers()
    {
        var invalidOffers = Game.CurrentTradeOffers.Where(x => x.Initiator == Initiator || x.Initiator == Target).ToList();
        foreach (var invalidOffer in invalidOffers) Game.CurrentTradeOffers.Remove(invalidOffer);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " offer to trade factions with ", Target);
    }

    #endregion Execution
}