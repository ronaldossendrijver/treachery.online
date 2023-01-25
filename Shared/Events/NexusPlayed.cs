/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public Faction Faction { get; set; }

        public PrescienceAspect GreenPrescienceAspect { get; set; }


        public override Message Validate()
        {
            switch (Faction)
            {
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

        public bool Cunning => IsCunning(Player);

        public bool SecretAlly => IsSecretAlly(Game, Player);

        public bool Betrayal => IsBetrayal(Game, Player);

        public static bool IsApplicable(Game g, Player p)
        {
            bool cunning = IsCunning(p);
            bool secretAlly = IsSecretAlly(g,p);
            bool betrayal = IsBetrayal(g,p);

            return (p.Nexus) switch
            {
                Faction.Green when betrayal => g.CurrentPhase == Phase.BattlePhase,
                Faction.Green when cunning || secretAlly => g.CurrentPhase == Phase.BattlePhase && g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p),

                Faction.Black when betrayal => g.CurrentPhase == Phase.CancellingTraitor,
                Faction.Black when cunning => true,
                Faction.Black when secretAlly => g.CurrentPhase == Phase.Contemplate,

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
