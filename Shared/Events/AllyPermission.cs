/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class AllyPermission : GameEvent
    {
        public AllyPermission(Game game) : base(game)
        {
        }

        public AllyPermission()
        {
        }

        public bool AllyMayShipAsOrange { get; set; }
        public int RedWillPayForExtraRevival { get; set; }
        public bool YellowWillProtectFromMonster { get; set; }
        public bool YellowAllowsThreeFreeRevivals { get; set; }
        public bool YellowSharesPrescience { get; set; }
        public bool AllyMayReviveAsPurple { get; set; }
        public bool AllyMayReplaceCards { get; set; }
        public bool GreenSharesPrescience { get; set; }
        public bool BlueAllowsUseOfVoice { get; set; }
        public bool WhiteAllowsUseOfNoField { get; set; }
        public int PermittedResources { get; set; }
        public int _permittedKarmaCardId { get; set; }

        [JsonIgnore]
        public TreacheryCard PermittedKarmaCard
        {
            get
            {
                return TreacheryCardManager.Lookup.Find(_permittedKarmaCardId);
            }
            set
            {
                if (value == null)
                {
                    _permittedKarmaCardId = -1;
                }
                else
                {
                    _permittedKarmaCardId = value.Id;
                }
            }
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
            return Message.Express(Initiator, " change ally permissions");
        }
    }
}
