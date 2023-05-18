/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class AllyPermission : GameEvent
    {
        #region Construction

        public AllyPermission(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public AllyPermission()
        {
        }

        #endregion Construction

        #region Properties

        //Needed for versions < 155
        public bool AllyMayShipAsOrange { get; set; }

        public bool OrangeAllowsShippingDiscount
        {
            get => AllyMayShipAsOrange;
            set => AllyMayShipAsOrange = value;
        }

        public int RedWillPayForExtraRevival { get; set; }

        public bool YellowWillProtectFromMonster { get; set; }

        public bool YellowAllowsThreeFreeRevivals { get; set; }

        public bool YellowSharesPrescience { get; set; }

        public bool YellowRefundsBattleDial { get; set; }

        public bool PurpleAllowsRevivalDiscount
        {
            get => AllyMayReviveAsPurple;
            set => AllyMayReviveAsPurple = value;
        }
        
        //Needed for versions < 155
        public bool AllyMayReviveAsPurple { get; set; }

        public bool GreyAllowsReplacingCards
        {
            get => AllyMayReplaceCards;
            set => AllyMayReplaceCards = value;
        }

        //Needed for versions < 155
        public bool AllyMayReplaceCards { get; set; }

        public bool GreenSharesPrescience { get; set; }

        public bool BlueAllowsUseOfVoice { get; set; }

        public bool WhiteAllowsUseOfNoField { get; set; }

        public bool CyanAllowsKeepingCards { get; set; }

        public bool PinkSharesAmbassadors { get; set; }

        public int PermittedResources { get; set; }

        public int _permittedKarmaCardId;

        [JsonIgnore]
        public TreacheryCard PermittedKarmaCard
        {
            get => TreacheryCardManager.Lookup.Find(_permittedKarmaCardId);
            set => _permittedKarmaCardId = TreacheryCardManager.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var ally = Player.Ally;

            switch (Initiator)
            {
                case Faction.Orange:
                    Game.OrangeAllowsShippingDiscount = OrangeAllowsShippingDiscount;
                    break;

                case Faction.Purple:
                    Game.PurpleAllowsRevivalDiscount = PurpleAllowsRevivalDiscount;
                    break;

                case Faction.Grey:
                    Game.GreyAllowsReplacingCards = GreyAllowsReplacingCards;
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

            Game.PermittedUseOfAllySpice.Set(ally, PermittedResources);
            Game.PermittedUseOfAllyKarma.Set(ally, PermittedKarmaCard);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " change ally permissions");
        }

        #endregion Execution
    }
}
