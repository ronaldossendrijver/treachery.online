/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class Bid : GameEvent
    {
        public int Amount { get; set; }

        public int AllyContributionAmount { get; set; }

        public int RedContributionAmount { get; set; }

        [JsonIgnore]
        public int TotalAmount
        {
            get
            {
                return Amount + AllyContributionAmount + RedContributionAmount;
            }
        }

        public bool Passed { get; set; }

        public int _karmaCardId = -1;

        public Bid(Game game) : base(game)
        {
        }

        public Bid()
        {
        }

        [JsonIgnore]
        public TreacheryCard KarmaCard
        {
            private get
            {
                return TreacheryCardManager.Lookup.Find(_karmaCardId);
            }

            set
            {
                if (value == null)
                {
                    _karmaCardId = -1;
                }
                else
                {
                    _karmaCardId = value.Id;
                }
            }
        }

        public TreacheryCard GetKarmaCard()
        {
            return KarmaCard;
        }

        /// <summary>
        /// This indicates Karma was used to remove the bid amount limit
        /// </summary>
        /// <param name="g"></param>
        [JsonIgnore]
        public bool UsingKarmaToRemoveBidLimit
        {
            get
            {
                return KarmaCard != null && !KarmaBid;
            }
        }

        /// <summary>
        /// This indicates the card is won immediately
        /// </summary>
        public bool KarmaBid { get; set; } = false;

        public override string Validate()
        {
            if (Passed || KarmaBid) return "";

            var p = Game.GetPlayer(Initiator);

            if (TotalAmount < 1) return "Bid must be higher than 0.";
            if (Game.CurrentBid != null && TotalAmount <= Game.CurrentBid.TotalAmount) return "Bid not high enough.";

            var ally = Game.GetPlayer(p.Ally);
            if (AllyContributionAmount > 0 && AllyContributionAmount > ally.Resources) return "Your ally can't pay that much.";

            var red = Game.GetPlayer(Faction.Red);
            if (RedContributionAmount > 0 && RedContributionAmount > red.Resources) return Skin.Current.Format("{0} can't pay that much.", Faction.Red);

            if (!UsingKarmaToRemoveBidLimit && Amount > Player.Resources) return "You can't pay that much.";
            if (KarmaCard != null && !Karma.ValidKarmaCards(Game, p).Contains(KarmaCard)) return Skin.Current.Format("Invalid {0} card.", TreacheryCardType.Karma);

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                if (KarmaBid)
                {
                    return new Message(Initiator, "{0} win the bid using {1}.", Initiator, TreacheryCardType.Karma);
                }
                else
                {
                    return new Message(Initiator, "{0} bid {1}.", Initiator, TotalAmount);
                }
            }
            else
            {
                return new Message(Initiator, "{0} pass.", Initiator);
            }
        }

        public static int ValidMaxAmount(Player p, bool usingKarma)
        {
            if (usingKarma)
            {
                return 100;
            }
            else
            {
                return p.Resources;
            }
        }

        public static int ValidMaxAllyAmount(Game g, Player p)
        {
            return g.SpiceYourAllyCanPay(p);
        }
    }
}
