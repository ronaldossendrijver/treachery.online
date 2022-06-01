/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region Permissions

        public void HandleEvent(AllyPermission e)
        {
            var ally = GetPlayer(e.Initiator).Ally;

            switch (e.Initiator)
            {
                case Faction.Orange:
                    OrangeAllyMayShipAsGuild = e.AllyMayShipAsOrange;
                    break;

                case Faction.Purple:
                    PurpleAllyMayReviveAsPurple = e.AllyMayReviveAsPurple;
                    break;

                case Faction.Grey:
                    GreyAllyMayReplaceCards = e.AllyMayReplaceCards;
                    break;

                case Faction.Red:
                    RedWillPayForExtraRevival = e.RedWillPayForExtraRevival;
                    break;

                case Faction.Yellow:
                    YellowWillProtectFromShaiHulud = e.YellowWillProtectFromMonster;
                    YellowAllowsThreeFreeRevivals = e.YellowAllowsThreeFreeRevivals;
                    YellowSharesPrescience = e.YellowSharesPrescience;
                    break;

                case Faction.Green:
                    GreenSharesPrescience = e.GreenSharesPrescience;
                    break;

                case Faction.Blue:
                    BlueAllyMayUseVoice = e.BlueAllowsUseOfVoice;
                    break;

                case Faction.White:
                    WhiteAllyMayUseNoField = e.WhiteAllowsUseOfNoField;
                    break;
            }

            Set(PermittedUseOfAllySpice, ally, e.PermittedResources);
            Set(PermittedUseOfAllyKarma, ally, e.PermittedKarmaCard);
        }

        public bool OrangeAllyMayShipAsGuild { get; private set; } = false;
        public bool PurpleAllyMayReviveAsPurple { get; private set; } = false;
        public bool GreyAllyMayReplaceCards { get; private set; } = false;
        public bool BlueAllyMayUseVoice { get; private set; } = false;
        public int RedWillPayForExtraRevival { get; private set; } = 0;
        public bool WhiteAllyMayUseNoField { get; private set; } = false;
        public bool YellowWillProtectFromShaiHulud { get; private set; } = false;
        public bool YellowAllowsThreeFreeRevivals { get; private set; } = false;
        public bool YellowSharesPrescience { get; private set; } = false;
        public bool GreenSharesPrescience { get; private set; } = false;

        private Dictionary<Faction, int> PermittedUseOfAllySpice { get; } = new Dictionary<Faction, int>();
        private Dictionary<Faction, int> PermittedUseOfRedSpice { get; set; } = new Dictionary<Faction, int>();
        private Dictionary<Faction, TreacheryCard> PermittedUseOfAllyKarma { get; } = new Dictionary<Faction, TreacheryCard>();

        #endregion

        #region BrownCardTrading

        public void HandleEvent(CardTraded e)
        {
            if (CurrentCardTradeOffer == null)
            {
                Log(e);
                CurrentCardTradeOffer = e;
                PhaseBeforeCardTrade = CurrentPhase;
                Enter(Phase.TradingCards);
            }
            else
            {
                Log(e.Initiator, " and ", CurrentCardTradeOffer.Initiator, " exchange a card");

                if (CurrentCardTradeOffer.Player.TreacheryCards.Count > 1 || e.Player.TreacheryCards.Count > 1)
                {
                    foreach (var p in Players.Where(pl => pl != CurrentCardTradeOffer.Player && pl != e.Player))
                    {
                        UnregisterKnown(p, CurrentCardTradeOffer.Player.TreacheryCards);
                        UnregisterKnown(p, e.Player.TreacheryCards);
                    }
                }

                CurrentCardTradeOffer.Player.TreacheryCards.Add(e.Card);
                e.Player.TreacheryCards.Remove(e.Card);
                e.Player.TreacheryCards.Add(CurrentCardTradeOffer.Card);
                CurrentCardTradeOffer.Player.TreacheryCards.Remove(CurrentCardTradeOffer.Card);
                CurrentCardTradeOffer = null;
                RecentMilestones.Add(Milestone.CardTraded);
                LastTurnCardWasTraded = CurrentTurn;
                Enter(PhaseBeforeCardTrade);
            }
        }
        public CardTraded CurrentCardTradeOffer { get; private set; } = null;
        private Phase PhaseBeforeCardTrade { get; set; } = Phase.None;
        private int LastTurnCardWasTraded { get; set; } = -1;

        #endregion

        #region WhiteCardTrading
        public void HandleEvent(WhiteGaveCard e)
        {
            var initiator = GetPlayer(e.Initiator);
            var target = initiator.AlliedPlayer;

            initiator.TreacheryCards.Remove(e.Card);
            RegisterKnown(initiator, e.Card);
            target.TreacheryCards.Add(e.Card);

            foreach (var p in Players.Where(p => p != initiator && p != target))
            {
                UnregisterKnown(p, initiator.TreacheryCards);
                UnregisterKnown(p, target.TreacheryCards);
            }

            Log(e);
        }

        #endregion

        #region Support

        private void Set<KeyType, ValueType>(IDictionary<KeyType, ValueType> dict, KeyType key, ValueType value)
        {
            if (dict.ContainsKey(key))
            {
                dict.Remove(key);
            }

            dict.Add(key, value);
        }

        private bool AreAllies(Faction a, Faction b)
        {
            var player = GetPlayer(a);
            if (player != null)
            {
                return player.Ally == b;
            }
            else
            {
                return false;
            }
        }

        public int SpiceYourAllyCanPay(Player p)
        {
            if (PermittedUseOfAllySpice.ContainsKey(p.Faction))
            {
                var ally = GetPlayer(p.Ally);
                return Math.Min(PermittedUseOfAllySpice[p.Faction], ally.Resources);
            }
            else
            {
                return 0;
            }
        }

        public int SpiceForBidsRedCanPay(Faction f)
        {
            if (PermittedUseOfRedSpice.ContainsKey(f))
            {
                var red = GetPlayer(Faction.Red);
                return Math.Min(PermittedUseOfRedSpice[f], red.Resources);
            }
            else
            {
                return 0;
            }
        }

        public TreacheryCard GetPermittedUseOfAllyKarma(Faction f)
        {
            var ally = Players.SingleOrDefault(p => p.Ally == f);

            if (PermittedUseOfAllyKarma.ContainsKey(f) && ally != null && ally.TreacheryCards.Contains(PermittedUseOfAllyKarma[f]))
            {
                return PermittedUseOfAllyKarma[f];
            }
            else
            {
                return null;
            }
        }

        public int GetPermittedUseOfAllySpice(Faction f)
        {
            var ally = GetPlayer(f).AlliedPlayer;

            if (!PermittedUseOfAllySpice.ContainsKey(f) || ally == null)
            {
                return 0;
            }
            else
            {
                return Math.Min(PermittedUseOfAllySpice[f], ally.Resources);
            }
        }

        private void DecreasePermittedUseOfAllySpice(Faction f, int amount)
        {
            if (PermittedUseOfAllySpice.ContainsKey(f))
            {
                var newValue = PermittedUseOfAllySpice[f] - amount;
                PermittedUseOfAllySpice[f] = Math.Max(0, newValue);
            }
        }

        #endregion
    }
}
