/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class KarmaPrescience : GameEvent
{
    #region Construction

    public KarmaPrescience(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public KarmaPrescience()
    {
    }

    #endregion Construction

    #region Validation

    public Faction Target { get; set; }
    public int PowerCardId { get; set; } = -1;

    [JsonIgnore]
    public Faction EffectiveTarget => Target != Faction.None ? Target :
        Game.CurrentBattle?.OpponentOf(Initiator)?.Faction ?? Game.CurrentBattle?.Defender ?? Faction.None;

    public override Message? Validate()
    {
        if (Game.SecondEdition && (Initiator != Faction.Green || Player.SpecialKarmaPowerUsed || !Karma.ValidPowerCards(Game, Player).Any() || Game.CurrentBattle == null))
            return Message.Express("Golden Path is not available");
        if (Game.SecondEdition && !ValidTargets(Game).Contains(EffectiveTarget)) return Message.Express("Select a battle plan to see");
        if (Game.SecondEdition && !Karma.IsValidPowerCard(Game, Player, PowerCardId)) return Message.Express("Select a card to invoke Golden Path");
        return null;
    }

    public static IEnumerable<Faction> ValidTargets(Game g) => g.CurrentBattle is { } battle
        ? new[] { battle.Aggressor, battle.Defender } : [];

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Discard(Player, Karma.PowerCard(Game, Player, PowerCardId));
        Player.SpecialKarmaPowerUsed = true;
        Log();
        Game.Stone(Milestone.Karma);
        Game.GreenKarma = true;
        if (Game.SecondEdition) Game.GoldenPathTarget = EffectiveTarget;
    }

    public override Message GetMessage()
    {
        return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " see the entire enemy battle plan");
    }

    #endregion Execution
}