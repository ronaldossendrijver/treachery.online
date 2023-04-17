/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class TerrorRevealed : PassableGameEvent, ILocationEvent
    {
        #region Construction

        public TerrorRevealed(Game game) : base(game)
        {
        }

        public TerrorRevealed()
        {
        }

        #endregion Construction

        #region Properties

        public bool AllianceOffered { get; set; }

        public TerrorType Type { get; set; }

        public int ForcesInSneakAttack { get; set; }

        public int _sneakAttackTo;

        [JsonIgnore]
        public Location SneakAttackTo
        {
            get => Game.Map.LocationLookup.Find(_sneakAttackTo);
            set => _sneakAttackTo = Game.Map.LocationLookup.GetId(value);
        }

        public bool RobberyTakesCard { get; set; }

        public int _cardToGiveInSabotageId;

        [JsonIgnore]
        public TreacheryCard CardToGiveInSabotage
        {
            get => TreacheryCardManager.Get(_cardToGiveInSabotageId);
            set => _cardToGiveInSabotageId = TreacheryCardManager.GetId(value);
        }

        [JsonIgnore]
        public Location To => SneakAttackTo;

        [JsonIgnore]
        public int TotalAmountOfForcesAddedToLocation => ForcesInSneakAttack;

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Passed && !MayPass(Game)) return Message.Express("You must reveal a terror token");
            if (AllianceOffered && !MayOfferAlliance(Game)) return Message.Express("You can't offer an alliance to this faction");

            if (Passed || AllianceOffered) return null;
            if (!GetTypes(Game).Contains(Type)) return Message.Express("You cannot use this token");
            if (Initiator != Faction.Cyan) return Message.Express("Your faction can't reveal terror tokens");

            if (Type == TerrorType.SneakAttack && SneakAttackTo == null && ForcesInSneakAttack > 0) return Message.Express("You cannot send forces there");
            if (Type == TerrorType.SneakAttack && SneakAttackTo != null && !ValidSneakAttackTargets(Game, Player).Contains(SneakAttackTo)) return Message.Express("Invalid location of sneak attack");
            if (Type == TerrorType.SneakAttack && ForcesInSneakAttack > MaxAmountOfForcesInSneakAttack(Player)) return Message.Express("Too many forces selected");

            return null;
        }

        public static bool MayPass(Game g) => !g.AllianceByTerrorWasOffered;

        public static bool MayOfferAlliance(Game g)
        {
            var victim = g.GetPlayer(GetVictim(g));
            var cyan = g.GetPlayer(Faction.Cyan);

            return !g.AllianceByTerrorWasOffered && !g.Prevented(FactionAdvantage.CyanEnemyOfEnemy) && !victim.Is(Faction.Pink) && !cyan.HaveForcesOnEachOthersHomeworlds(victim);
        }

        public static Territory GetTerritory(Game g) => g.LastTerrorTrigger?.Territory;

        public static Faction GetVictim(Game g) => g.LastTerrorTrigger != null ? g.LastTerrorTrigger.Initiator : Faction.None;

        public static IEnumerable<TerrorType> GetTypes(Game g) => g.LastTerrorTrigger != null ? g.TerrorIn(GetTerritory(g)) : Array.Empty<TerrorType>();

        public static int MaxAmountOfForcesInSneakAttack(Player p) => Math.Min(5, p.ForcesInReserve);

        public static IEnumerable<Location> ValidSneakAttackTargets(Game g, Player p) => GetTerritory(g).Locations.Where(l => OpenDespiteAllyAndStormAndOccupancy(g, p, l));

        private static bool OpenDespiteAllyAndStormAndOccupancy(Game g, Player p, Location l) =>
            g.IsNotFull(p, l) &&
            l.Sector != g.SectorInStorm &&
            (!p.HasAlly || p.AlliedPlayer.AnyForcesIn(l.Territory) == 0 || p.Ally == Faction.Blue && g.Applicable(Rule.AdvisorsDontConflictWithAlly) && p.AlliedPlayer.ForcesIn(l.Territory) == 0);

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var territory = GetTerritory(Game);
            var victim = GetVictim(Game);
            var victimPlayer = GetPlayer(victim);

            if (Passed)
            {
                Log(Initiator, " don't terrorize ", territory);
                Game.AllianceByTerrorWasOffered = false;
                Game.DequeueIntrusion(IntrusionType.Terror);
                Game.DetermineNextShipmentAndMoveSubPhase();
            }
            else if (AllianceOffered)
            {
                Log(Initiator, " offer an alliance to ", victim, " as an alternative to terror");
                Game.AllianceByTerrorOfferedTo = victim;
                Game.AllianceByTerrorWasOffered = true;
                Game.PausedTerrorPhase = Game.CurrentPhase;
                Game.Enter(Phase.AllianceByTerror);
            }
            else
            {
                Game.Stone(Milestone.TerrorRevealed);

                Game.TerrorOnPlanet.Remove(Type);

                if (Passed || !Game.TerrorIn(territory).Any())
                {
                    Game.DequeueIntrusion(IntrusionType.Terror);
                }

                Game.AllianceByTerrorWasOffered = false;

                switch (Type)
                {
                    case TerrorType.Assassination:

                        var randomLeader = victimPlayer.Leaders.RandomOrDefault(Game.Random);
                        if (randomLeader != null)
                        {
                            Log(Initiator, " gain ", Payment.Of(randomLeader.CostToRevive), " from assassinating ", randomLeader, " in ", territory);
                            Player.Resources += randomLeader.CostToRevive;
                            Game.KillHero(randomLeader);
                        }
                        else
                        {
                            Log(Initiator, " fail to assassinate a leader in ", territory);
                        }
                        break;

                    case TerrorType.Atomics:

                        Game.KillAllForcesIn(territory, false);
                        Game.KillAmbassadorIn(territory);
                        Game.AtomicsAftermath = territory;

                        if (Player.TreacheryCards.Count > Player.MaximumNumberOfCards)
                        {
                            Game.Discard(Player, Player.TreacheryCards.RandomOrDefault(Game.Random));
                        }

                        Game.Stone(Milestone.MetheorUsed);
                        Log(Initiator, " DETONATE ATOMICS in ", territory);
                        break;

                    case TerrorType.Extortion:

                        Log(Initiator, " will get ", Payment.Of(5), " from ", Type, " during ", MainPhase.Contemplate);
                        Player.Extortion += 5;
                        break;

                    case TerrorType.Robbery:

                        if (RobberyTakesCard)
                        {
                            Player.TreacheryCards.Add(Game.DrawTreacheryCard());
                            Log(Initiator, " draw a Treachery Card");
                        }
                        else
                        {
                            var amountStolen = (int)Math.Ceiling(0.5f * victimPlayer.Resources);
                            Player.Resources += amountStolen;
                            victimPlayer.Resources -= amountStolen;
                            Log(Initiator, " steal ", Payment.Of(amountStolen), " by ", Type, " in ", territory);
                        }
                        break;

                    case TerrorType.Sabotage:

                        Log(Initiator, " sabotage ", victimPlayer.Faction);

                        if (victimPlayer.TreacheryCards.Any())
                        {
                            Game.Discard(victimPlayer.TreacheryCards.RandomOrDefault(Game.Random));
                        }
                        else
                        {
                            Log(victimPlayer.Faction, " have no treachery cards to discard");
                        }

                        if (CardToGiveInSabotage != null)
                        {
                            Player.TreacheryCards.Remove(CardToGiveInSabotage);
                            victimPlayer.TreacheryCards.Add(CardToGiveInSabotage);
                            Log(Initiator, " give a treachery card to ", victimPlayer.Faction);
                        }

                        break;

                    case TerrorType.SneakAttack:

                        if (SneakAttackTo != null)
                        {
                            Log(Initiator, " sneak attack ", territory, " with ", ForcesInSneakAttack, Player.Force);
                            Player.ShipForces(SneakAttackTo, ForcesInSneakAttack);
                            Game.CheckIntrusion(this);
                        }
                        break;

                }

                Game.DetermineNextShipmentAndMoveSubPhase();
            }

            if (!Passed && Type == TerrorType.Robbery && RobberyTakesCard && Player.TreacheryCards.Count > Player.MaximumNumberOfCards)
            {
                Game.LetPlayerDiscardTreacheryCardOfChoice(Initiator);
            }
        }

        public override Message GetMessage()
        {
            if (AllianceOffered)
            {
                return Message.Express(Initiator, " offer an alliance instead of terror");
            }
            else if (Passed)
            {
                return Message.Express(Initiator, " don't terrorize");
            }
            else
            {
                return Message.Express(Initiator, " reveal a terror token");
            }
        }

        #endregion Execution
    }
}
