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

        public static bool IsCunning(Player p) => p.Faction == p.Nexus;

        public static bool IsSecretAlly(Game g, Player p) => !g.IsPlaying(p.Nexus);

        public static bool IsBetrayal(Game g, Player p) => !(IsCunning(p) || IsSecretAlly(g, p));

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

            bool cunning = IsCunning(p);
            bool secretAlly = IsSecretAlly(g,p);
            bool betrayal = !(cunning || secretAlly);

            return (p.Nexus) switch
            {
                Faction.Green when betrayal => g.CurrentPhase == Phase.BattlePhase,
                Faction.Green when cunning || secretAlly => g.CurrentPhase == Phase.BattlePhase && g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p),

                Faction.Black when betrayal => g.CurrentPhase == Phase.CancellingTraitor,
                Faction.Black when cunning => true,
                Faction.Black when secretAlly => g.CurrentPhase == Phase.Contemplate,

                Faction.Yellow when betrayal => g.CurrentMainPhase == MainPhase.Blow || g.CurrentMainPhase == MainPhase.ShipmentAndMove,
                Faction.Yellow when cunning => g.CurrentMainPhase == MainPhase.Blow,
                Faction.Yellow when secretAlly => g.CurrentMainPhase == MainPhase.Blow || g.CurrentMainPhase == MainPhase.Resurrection,

                _ => false
            };



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
