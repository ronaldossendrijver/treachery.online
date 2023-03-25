/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region Nexus

        public readonly IList<AllianceOffered> CurrentAllianceOffers = new List<AllianceOffered>();

        internal void MakeAlliance(Faction a, Faction b)
        {
            var playerA = GetPlayer(a);
            var playerB = GetPlayer(b);
            playerA.Ally = b;
            playerB.Ally = a;
            DiscardNexusCard(playerA);
            DiscardNexusCard(playerB);
            Log(a, " and ", b, " are now allies");

            if (Version > 150)
            {
                SetPermissions(a, true);
                SetPermissions(b, true);
            }
        }

        internal void BreakAlliance(Faction f)
        {
            var initiator = GetPlayer(f);
            var currentAlly = GetPlayer(initiator.Ally);

            if (Version <= 150)
            {
                if (f == Faction.Orange || initiator.Ally == Faction.Orange)
                {
                    AllyMayShipAsOrange = false;
                }

                if (f == Faction.Red || initiator.Ally == Faction.Red)
                {
                    RedWillPayForExtraRevival = 0;
                }

                if (f == Faction.Yellow || initiator.Ally == Faction.Yellow)
                {
                    YellowWillProtectFromMonster = false;
                    YellowAllowsThreeFreeRevivals = false;
                }
            }
            else
            {
                SetPermissions(f, false);
                SetPermissions(initiator.Ally, false);
            }

            PermittedUseOfAllySpice.Remove(f);
            PermittedUseOfAllySpice.Remove(initiator.Ally);
            PermittedUseOfAllyKarma.Remove(f);
            PermittedUseOfAllyKarma.Remove(initiator.Ally);

            initiator.Ally = Faction.None;
            currentAlly.Ally = Faction.None;
        }

        private void SetPermissions(Faction f, bool enabled)
        {
            switch (f)
            {
                case Faction.Orange: AllyMayShipAsOrange = enabled; break;
                case Faction.Purple: AllyMayReviveAsPurple = enabled; break;
                case Faction.Grey: AllyMayReplaceCards = enabled; break;
                case Faction.Blue: BlueAllowsUseOfVoice = enabled; break;
                case Faction.Red: RedWillPayForExtraRevival = enabled ? 3 : 0; break;
                case Faction.White: WhiteAllowsUseOfNoField = enabled; break;
                case Faction.Yellow:
                    YellowWillProtectFromMonster = enabled;
                    YellowAllowsThreeFreeRevivals = enabled;
                    YellowSharesPrescience = enabled;
                    YellowRefundsBattleDial = enabled;
                    break;
                case Faction.Green: GreenSharesPrescience = enabled; break;
                case Faction.Cyan: CyanAllowsKeepingCards = enabled; break;
                case Faction.Pink: PinkSharesAmbassadors = enabled; break;
            }
        }

        private bool NexusHasOccured { get; set; } = false;

        private void EndNexus()
        {
            NexusHasOccured = true;
            CurrentAllianceOffers.Clear();

            if (YellowRidesMonster.IsApplicable(this))
            {
                Enter(CurrentPhase == Phase.AllianceA, Phase.YellowRidingMonsterA, Phase.YellowRidingMonsterB);
            }
            else
            {
                if (CurrentPhase == Phase.AllianceA)
                {
                    Enter(Applicable(Rule.IncreasedResourceFlow), EnterBlowB, StartNexusCardPhase);
                }
                else if (CurrentPhase == Phase.AllianceB)
                {
                    StartNexusCardPhase();
                }
            }
        }

        #endregion

        #region Permissions

        public bool AllyMayShipAsOrange { get; internal set; } = false;
        public bool AllyMayReviveAsPurple { get; internal set; } = false;
        public bool AllyMayReplaceCards { get; internal set; } = false;
        public bool BlueAllowsUseOfVoice { get; internal set; } = false;
        public int RedWillPayForExtraRevival { get; internal set; } = 0;
        public bool WhiteAllowsUseOfNoField { get; internal set; } = false;
        public bool YellowWillProtectFromMonster { get; internal set; } = false;
        public bool YellowAllowsThreeFreeRevivals { get; internal set; } = false;
        public bool YellowSharesPrescience { get; internal set; } = false;
        public bool YellowRefundsBattleDial { get; internal set; } = false;
        public bool GreenSharesPrescience { get; internal set; } = false;
        public bool CyanAllowsKeepingCards { get; internal set; } = false;
        public bool PinkSharesAmbassadors { get; internal set; } = false;

        internal Dictionary<Faction, int> PermittedUseOfAllySpice { get; } = new();
        internal Dictionary<Faction, int> PermittedUseOfRedSpice { get; set; } = new();
        internal Dictionary<Faction, TreacheryCard> PermittedUseOfAllyKarma { get; } = new();

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
                Stone(Milestone.CardTraded);
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

        internal void Set<KeyType, ValueType>(IDictionary<KeyType, ValueType> dict, KeyType key, ValueType value)
        {
            dict.Remove(key);
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
