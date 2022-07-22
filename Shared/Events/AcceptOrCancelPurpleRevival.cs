/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class AcceptOrCancelPurpleRevival : GameEvent
    {
        public bool Cancel;

        public int Price;

        public int _heroId;

        [JsonIgnore]
        public IHero Hero
        {
            get
            {
                return LeaderManager.HeroLookup.Find(_heroId);
            }
            set
            {
                _heroId = LeaderManager.HeroLookup.GetId(value);
            }
        }

        [JsonIgnore]
        private bool IsDenial => Price == int.MaxValue;

        public AcceptOrCancelPurpleRevival(Game g) : base(g) { }

        public AcceptOrCancelPurpleRevival()
        {
        }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (!Cancel)
            {
                if (IsDenial)
                {
                    return Message.Express(Initiator, " deny ", Hero.Faction, " early revival of a leader");
                }
                else
                {
                    return Message.Express(Initiator, " offer ", Hero.Faction, " revival of a leader for ", new Payment(Price));
                }
            }
            else
            {
                return Message.Express(Initiator, " cancel their revival offer to ", Hero.Faction);
            }
        }

        public static int MinAmount()
        {
            return 0;
        }

        public static int MaxAmount()
        {
            return 100;
        }
    }
}
