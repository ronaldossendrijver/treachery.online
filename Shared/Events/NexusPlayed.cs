/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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

        public int PurpleForces { get; set; }

        public int PurpleSpecialForces { get; set; }

        public int _purpleHeroId = -1;

        [JsonIgnore]
        public IHero PurpleHero
        {
            get
            {
                return LeaderManager.HeroLookup.Find(_purpleHeroId);
            }
            set
            {
                _purpleHeroId = LeaderManager.HeroLookup.GetId(value);
            }
        }

        public bool PurpleAssignSkill { get; set; } = false;

        public int _brownCardId;

        [JsonIgnore]
        public TreacheryCard BrownCard
        {
            get
            {
                return TreacheryCardManager.Get(_brownCardId);
            }
            set
            {
                _brownCardId = TreacheryCardManager.GetId(value);
            }
        }

        public override Message Validate()
        {
            switch (Faction)
            {
                case Faction.None: return Message.Express("Invalid Nexus faction");

                case Faction.Green:
                    if (GreenPrescienceAspect == PrescienceAspect.None) return Message.Express("Invalid battle plan element");
                    break;

                case Faction.Purple:
                    if (PurpleHero != null && !ValidPurpleHeroes(Game, Player).Contains(PurpleHero)) return Message.Express("Invalid leader");
                    if (PurpleForces > ValidPurpleMaxAmount(Game, Player, false)) return Message.Express("You can't revive that many ", Player.Force);
                    if (PurpleSpecialForces > ValidPurpleMaxAmount(Game, Player, true)) return Message.Express("You can't revive that many ", Player.SpecialForce);
                    if (DeterminePurpleCost() > Player.Resources) return Message.Express("You can't pay that many");
                    if (PurpleForces + PurpleSpecialForces > 5) return Message.Express("You can't revive that many forces");
                    if (PurpleAssignSkill && PurpleHero == null) return Message.Express("You must revive a leader to assign a skill to");
                    if (PurpleAssignSkill && !Revival.MayAssignSkill(Game, Player, PurpleHero)) return Message.Express("You can't assign a skill to this leader");
                    break;

                case Faction.Brown:
                    if (BrownCard == null) return Message.Express("Select a ", TreacheryCardType.Useless, " card to discard");
                    if (BrownCard != null && !ValidBrownCards(Player).Contains(BrownCard)) return Message.Express("Invalid card");
                    break;
            }


            return null;
        }

        public static bool CanUseCunning(Player p) => p.Nexus != Faction.None && p.Nexus == p.Faction;

        public static bool CanUseSecretAlly(Game g, Player p) => p.Nexus != Faction.None && !g.IsPlaying(p.Nexus);

        public static bool CanUseBetrayal(Game g, Player p) => p.Nexus != Faction.None && p.Nexus != p.Faction && g.IsPlaying(p.Nexus);

        [JsonIgnore]
        public bool IsCunning => Initiator == Faction;

        [JsonIgnore]
        public bool IsSecretAlly => !Game.IsPlaying(Faction);

        [JsonIgnore]
        public bool IsBetrayal => !(IsCunning && IsSecretAlly);

        public static bool IsApplicable(Game g, Player p)
        {
            if (g.CurrentPhase == Phase.NexusCards || g.CurrentPhaseIsUnInterruptable)
            {
                return false;
            }

            bool cunning = CanUseCunning(p);
            bool secretAlly = CanUseSecretAlly(g, p);
            bool betrayal = CanUseBetrayal(g, p);

            bool gameIsInBattle = g.CurrentPhase == Phase.BattlePhase && g.CurrentBattle != null;
            bool isCurrentlyFormulatingBattlePlan = gameIsInBattle && g.CurrentBattle.IsAggressorOrDefender(p) && (g.DefenderBattleAction == null || g.AggressorBattleAction == null);

            return (p.Nexus) switch
            {
                Faction.Green when betrayal => gameIsInBattle && g.CurrentBattle.IsAggressorOrDefender(Faction.Green),
                Faction.Green when cunning || secretAlly => isCurrentlyFormulatingBattlePlan,

                Faction.Black when betrayal => g.CurrentPhase == Phase.CancellingTraitor,
                Faction.Black when cunning => true,
                Faction.Black when secretAlly => g.CurrentPhase == Phase.Contemplate,

                Faction.Yellow when betrayal => g.CurrentMainPhase == MainPhase.Blow || g.CurrentMainPhase == MainPhase.ShipmentAndMove,
                Faction.Yellow when cunning => g.CurrentMainPhase == MainPhase.Blow && g.MonsterAppearedInTerritoryWithoutForces,
                Faction.Yellow when secretAlly => g.CurrentMainPhase == MainPhase.Blow || g.CurrentMainPhase == MainPhase.Resurrection,

                Faction.Red when betrayal => g.CurrentMainPhase == MainPhase.Bidding || gameIsInBattle && g.Applicable(Rule.RedSpecialForces) && g.CurrentBattle.IsAggressorOrDefender(Faction.Red),
                Faction.Red when cunning => isCurrentlyFormulatingBattlePlan,

                Faction.Orange when betrayal => g.CurrentMainPhase == MainPhase.ShipmentAndMove && g.RecentlyPaid != null && g.HasRecentPaymentFor(typeof(Shipment)),
                Faction.Orange when cunning => g.CurrentPhase == Phase.OrangeMove && !g.InOrangeCunningShipment,
                Faction.Orange when secretAlly => g.CurrentPhase == Phase.NonOrangeShip,

                Faction.Blue when betrayal => gameIsInBattle && g.CurrentBattle.IsAggressorOrDefender(Faction.Blue),
                Faction.Blue when cunning => g.CurrentMainPhase == MainPhase.ShipmentAndMove,
                Faction.Blue when secretAlly => g.CurrentPhase == Phase.BattlePhase && g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p),

                Faction.Grey when betrayal => g.CurrentPhase == Phase.BeginningOfBidding || g.CurrentPhase > Phase.BeginningOfBidding && g.CurrentPhase < Phase.BiddingReport,
                Faction.Grey when cunning => isCurrentlyFormulatingBattlePlan,

                Faction.Purple when betrayal => g.CurrentPhase == Phase.Facedancing,
                Faction.Purple when cunning => true,
                Faction.Purple when secretAlly => g.CurrentPhase == Phase.Resurrection,

                Faction.Brown when betrayal => true,
                Faction.Brown when secretAlly => g.CurrentMainPhase == MainPhase.Collection && ValidBrownCards(p).Any() || g.CurrentPhase == Phase.BattleConclusion && g.CurrentBattle != null && p.Faction == g.BattleWinner,

                Faction.White when betrayal => g.CurrentMainPhase == MainPhase.Bidding && g.WhiteBiddingJustFinished && g.CardJustWon != null,
                Faction.Orange when cunning => g.CurrentPhase == Phase.OrangeMove && !g.InOrangeCunningShipment,
                Faction.Orange when secretAlly => g.CurrentPhase == Phase.NonOrangeShip,

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

        public int DeterminePurpleCost()
        {
            return DeterminePurpleCost(PurpleForces, PurpleSpecialForces);
        }

        public static int DeterminePurpleCost(int Forces, int SpecialForces)
        {
            return (Forces + SpecialForces);
        }

        public static int ValidPurpleMaxAmount(Game g, Player p, bool specialForces)
        {
            if (specialForces)
            {
                if (p.Faction == Faction.Red || p.Faction == Faction.Yellow)
                {
                    if (g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction))
                    {
                        return 0;
                    }
                    else
                    {
                        return Math.Min(p.SpecialForcesKilled, 1);
                    }
                }
                else
                {
                    return Math.Min(p.SpecialForcesKilled, 5);
                }
            }
            else
            {
                return Math.Min(p.ForcesKilled, 5);
            }
        }

        public static IEnumerable<IHero> ValidPurpleHeroes(Game game, Player player) => game.KilledHeroes(player);

        public static IEnumerable<TreacheryCard> ValidBrownCards(Player player) => player.TreacheryCards.Where(c => c.Type == TreacheryCardType.Useless);
    }
}
