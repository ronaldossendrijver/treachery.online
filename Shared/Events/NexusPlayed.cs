/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class NexusPlayed : GameEvent
    {
        public NexusPlayed(Game game) : base(game)
        {
        }

        public NexusPlayed()
        {
        }

        public PrescienceAspect GreenPrescienceAspect { get; set; }

        public Faction Faction { get; set; }

        public override Message Validate()
        {
            switch (Faction)
            {
                case Faction.None: return Message.Express("Invalid Nexus faction");
                    
                case Faction.Brown:
                    break;

                case Faction.Pink:
                    break;

                case Faction.Yellow:
                    break;

                case Faction.Grey:
                    break;

                case Faction.White:
                    break;

                case Faction.Orange:
                    break;

                case Faction.Purple:
                    break;
            }
            

            return null;
        }

        public static bool CanUseCunning(Player p) => p.Faction == p.Nexus;

        public static bool CanUseSecretAlly(Game g, Player p) => !g.IsPlaying(p.Nexus);

        public static bool CanUseBetrayal(Game g, Player p) => !(CanUseCunning(p) || CanUseSecretAlly(g, p));

        [JsonIgnore]
        public bool Cunning => Initiator == Faction;

        [JsonIgnore]
        public bool SecretAlly => !Game.IsPlaying(Faction);

        [JsonIgnore]
        public bool Betrayal => !(Cunning && SecretAlly);

        public static bool IsApplicable(Game g, Player p)
        {
            if (g.CurrentPhase == Phase.NexusCards)
            {
                return false;
            }

            bool cunning = CanUseCunning(p);
            bool secretAlly = CanUseSecretAlly(g,p);
            bool betrayal = !(cunning || secretAlly);

            bool isCurrentlyFormulatingBattlePlan = g.CurrentPhase == Phase.BattlePhase && g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p) && (g.DefenderBattleAction == null || g.AggressorBattleAction == null);

            return (p.Nexus) switch
            {
                Faction.Green when betrayal => g.CurrentPhase == Phase.BattlePhase,
                Faction.Green when cunning || secretAlly => isCurrentlyFormulatingBattlePlan,

                Faction.Black when betrayal => g.CurrentPhase == Phase.CancellingTraitor,
                Faction.Black when cunning => true,
                Faction.Black when secretAlly => g.CurrentPhase == Phase.Contemplate,

                Faction.Yellow when betrayal => g.CurrentMainPhase == MainPhase.Blow || g.CurrentMainPhase == MainPhase.ShipmentAndMove,
                Faction.Yellow when cunning => g.CurrentMainPhase == MainPhase.Blow && g.MonsterAppearedInTerritoryWithoutForces,
                Faction.Yellow when secretAlly => g.CurrentMainPhase == MainPhase.Blow || g.CurrentMainPhase == MainPhase.Resurrection,

                Faction.Red when betrayal => g.CurrentMainPhase == MainPhase.Bidding || g.CurrentMainPhase == MainPhase.Battle && g.Applicable(Rule.RedSpecialForces) && g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(Faction.Red),
                Faction.Red when cunning => isCurrentlyFormulatingBattlePlan,

                Faction.Orange when betrayal => g.RecentlyPaid != null && g.HasRecentPaymentFor(typeof(Shipment)),
                Faction.Orange when cunning => g.CurrentPhase == Phase.OrangeMove && !g.InOrangeCunningShipment,
                Faction.Orange when secretAlly => g.CurrentPhase == Phase.NonOrangeShip,

                Faction.Blue when betrayal => g.CurrentPhase == Phase.BattlePhase,
                Faction.Blue when cunning => g.CurrentMainPhase == MainPhase.ShipmentAndMove,
                Faction.Blue when secretAlly => g.CurrentPhase == Phase.BattlePhase && g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p),

                Faction.Grey when betrayal => g.CurrentPhase == Phase.BeginningOfBidding || g.CurrentPhase > Phase.BeginningOfBidding && g.CurrentPhase < Phase.BiddingReport,
                Faction.Grey when cunning => isCurrentlyFormulatingBattlePlan,

                Faction.Purple when betrayal => g.CurrentPhase == Phase.Facedancing,

                _ => false
            } ;



        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " play a Nexus card");
        }

    }

}
