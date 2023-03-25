/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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
        public bool YellowRefundsBattleDial { get; set; }
        public bool AllyMayReviveAsPurple { get; set; }
        public bool AllyMayReplaceCards { get; set; }
        public bool GreenSharesPrescience { get; set; }
        public bool BlueAllowsUseOfVoice { get; set; }
        public bool WhiteAllowsUseOfNoField { get; set; }
        public bool CyanAllowsKeepingCards { get; set; }
        public bool PinkSharesAmbassadors { get; set; }

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
            var ally = Player.Ally;

            switch (Initiator)
            {
                case Faction.Orange:
                    Game.AllyMayShipAsOrange = AllyMayShipAsOrange;
                    break;

                case Faction.Purple:
                    Game.AllyMayReviveAsPurple = AllyMayReviveAsPurple;
                    break;

                case Faction.Grey:
                    Game.AllyMayReplaceCards = AllyMayReplaceCards;
                    break;

                case Faction.Red:
                    Game.RedWillPayForExtraRevival = RedWillPayForExtraRevival;
                    break;

                case Faction.Yellow:
                    Game.YellowWillProtectFromMonster = YellowWillProtectFromMonster;
                    Game.YellowAllowsThreeFreeRevivals = YellowAllowsThreeFreeRevivals;
                    Game.YellowSharesPrescience = YellowSharesPrescience;
                    Game.YellowRefundsBattleDial = YellowRefundsBattleDial;
                    break;

                case Faction.Green:
                    Game.GreenSharesPrescience = GreenSharesPrescience;
                    break;

                case Faction.Blue:
                    Game.BlueAllowsUseOfVoice = BlueAllowsUseOfVoice;
                    break;

                case Faction.White:
                    Game.WhiteAllowsUseOfNoField = WhiteAllowsUseOfNoField;
                    break;

                case Faction.Cyan:
                    Game.CyanAllowsKeepingCards = CyanAllowsKeepingCards;
                    break;

                case Faction.Pink:
                    Game.PinkSharesAmbassadors = PinkSharesAmbassadors;
                    break;

            }

            Game.Set(Game.PermittedUseOfAllySpice, ally, PermittedResources);
            Game.Set(Game.PermittedUseOfAllyKarma, ally, PermittedKarmaCard);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " change ally permissions");
        }
    }
}
