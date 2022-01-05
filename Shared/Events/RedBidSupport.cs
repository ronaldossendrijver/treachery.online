/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class RedBidSupport : GameEvent
    {
        public RedBidSupport(Game game) : base(game)
        {
        }

        public RedBidSupport()
        {
        }

        //public Faction[] Factions { get; set; }

        public Dictionary<Faction, int> Amounts { get; set; }

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Amounts.Sum(kvp => kvp.Value) > 0)
            {
                return new Message(Initiator, "{0} supports bids by: {1}.", Initiator, string.Join(", ", Amounts.Where(kvp => kvp.Value > 0).Select(f => Skin.Current.Describe(f.Key) + " (" + f.Value + ")")));
            }
            else
            {
                return new Message(Initiator, "{0} doesn't support bids by opponents.", Initiator);
            }
        }


        /*
        [JsonIgnore]
        public Dictionary<Faction, int> AmountsOfFactions
        {
            get {

                var result = new Dictionary<Faction, int>();
                for (int i = 0; i < Factions.Length; i++)
                {
                    result.Add(Factions[i], Amounts[i]);
                }
                return result;
            }

            set
            {
                Factions = value.Keys.ToArray();
                Amounts = new int[Factions.Length];
                for (int i = 0; i < Factions.Length; i++)
                {
                    Amounts[i] = value[Factions[i]];
                }
            }
        }*/
    }
}
