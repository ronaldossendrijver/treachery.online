/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;

namespace Treachery.Client;

/// <summary>
/// Determines which treachery card in hand is used by game events that are played by clicking a card in the player panel.
/// </summary>
public static class CardActions
{
    public static TreacheryCard? CardUsedBy(Type action, Game game, Player player) => action.Name switch
    {
        nameof(ThumperPlayed) => player.Card(TreacheryCardType.Thumper),
        nameof(RaiseDeadPlayed) => player.Card(TreacheryCardType.RaiseDead),
        nameof(StormSpellPlayed) => player.Card(TreacheryCardType.StormSpell),
        nameof(MetheorPlayed) => player.Card(TreacheryCardType.Metheor),
        nameof(Caravan) => player.Card(TreacheryCardType.Caravan),
        nameof(AmalPlayed) => player.Card(TreacheryCardType.Amal),
        nameof(RecruitsPlayed) => player.Card(TreacheryCardType.Recruits),
        nameof(HarvesterPlayed) => player.Card(TreacheryCardType.Harvester),
        nameof(DiscardedSearchedAnnounced) or nameof(DiscardedSearched) => player.Card(TreacheryCardType.SearchDiscarded),
        nameof(DiscardedTaken) => player.Card(TreacheryCardType.TakeDiscarded),
        nameof(DistransUsed) => player.Card(TreacheryCardType.Distrans),
        nameof(FlightUsed) => player.Card(TreacheryCardType.Flight),
        nameof(JuicePlayed) => player.Card(TreacheryCardType.Juice),
        nameof(ResidualPlayed) => player.Card(TreacheryCardType.Residual),
        nameof(PortableAntidoteUsed) => player.Card(TreacheryCardType.PortableAntidote),
        nameof(PoisonToothCancelled) => BattleWeapon(game, player, c => c.Type == TreacheryCardType.PoisonTooth) ?? player.Card(TreacheryCardType.PoisonTooth),
        nameof(RockWasMelted) => BattleWeapon(game, player, c => c.IsRockMelter) ?? player.Card(TreacheryCardType.Rockmelter),
        nameof(ClairVoyancePlayed) => ClairvoyanceCard(game, player),
        nameof(BrownExtraMove) => NexusPlayed.CanUseCunning(player) ? null : BrownExtraMove.CardToUse(player),
        nameof(BrownMovePrevention) => NexusPlayed.CanUseCunning(player) ? null : BrownMovePrevention.CardToUse(player),
        nameof(BrownKarmaPrevention) => NexusPlayed.CanUseCunning(player) ? null : BrownKarmaPrevention.CardToUse(player),
        nameof(BrownFreeRevivalPrevention) => NexusPlayed.CanUseCunning(player) ? null : BrownFreeRevivalPrevention.CardToUse(player),
        nameof(BrownRemoveForce) => BrownRemoveForce.CardToUse(player),
        _ => null
    };

    private static TreacheryCard? BattleWeapon(Game game, Player player, Func<TreacheryCard, bool> predicate)
    {
        var weapon = game.CurrentBattle?.PlanOf(player)?.Weapon;
        return weapon != null && predicate(weapon) ? weapon : null;
    }

    // Mirrors the card selection performed by ClairVoyancePlayed when it is executed.
    private static TreacheryCard? ClairvoyanceCard(Game game, Player player)
    {
        var inShrine = player.Occupies(game.Map.Shrine);
        TreacheryCard? result = null;

        if (inShrine)
            result = Karma.ValidKarmaCards(game, player).FirstOrDefault(c => c.Type == TreacheryCardType.Useless);

        result ??= player.Card(TreacheryCardType.Clairvoyance);

        if (result == null && inShrine)
            result = Karma.ValidKarmaCards(game, player).FirstOrDefault();

        return result;
    }
}
