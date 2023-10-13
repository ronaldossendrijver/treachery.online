/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class AmbassadorActivated : PassableGameEvent, ILocationEvent, IPlacement
    {
        #region Construction

        public AmbassadorActivated(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public AmbassadorActivated()
        {
        }

        #endregion Construction

        #region Properties

        public Ambassador BlueSelectedAmbassador { get; set; }

        public string _brownCardIds;

        [JsonIgnore]
        public IEnumerable<TreacheryCard> BrownCards
        {
            get => IdStringToObjects(_brownCardIds, TreacheryCardManager.Lookup);
            set => _brownCardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
        }

        public bool PinkOfferAlliance { get; set; }

        public bool PinkGiveVidalToAlly { get; set; }

        public bool PinkTakeVidal { get; set; }

        public int _yellowOrOrangeToId;

        [JsonIgnore]
        public Location YellowOrOrangeTo
        {
            get => Game.Map.LocationLookup.Find(_yellowOrOrangeToId);
            set => _yellowOrOrangeToId = Game.Map.LocationLookup.GetId(value);
        }

        [JsonIgnore]
        public Location To => YellowOrOrangeTo;

        public string _yellowForceLocations = "";

        [JsonIgnore]
        public Dictionary<Location, Battalion> YellowForceLocations
        {
            get => PlacementEvent.ParseForceLocations(Game, Player.Faction, _yellowForceLocations);
            set => _yellowForceLocations = PlacementEvent.ForceLocationsString(Game, value);
        }

        public int _greyCardId;

        [JsonIgnore]
        public TreacheryCard GreyCard
        {
            get => TreacheryCardManager.Get(_greyCardId);
            set => _greyCardId = TreacheryCardManager.GetId(value);
        }

        public int OrangeForceAmount { get; set; }

        public int PurpleAmountOfForces { get; set; }

        public int _purpleHeroId = -1;

        [JsonIgnore]
        public IHero PurpleHero
        {
            get => LeaderManager.HeroLookup.Find(_purpleHeroId);
            set => _purpleHeroId = LeaderManager.HeroLookup.GetId(value);
        }

        public bool PurpleAssignSkill { get; set; }

        [JsonIgnore]
        public int TotalAmountOfForcesAddedToLocation => YellowForceLocations != null ? YellowForceLocations.Values.Sum(b => b.TotalAmountOfForces) : 0;

        [JsonIgnore]
        public int ForcesAddedToLocation => YellowForceLocations != null ? YellowForceLocations.Values.Sum(b => b.AmountOfForces) : 0;

        [JsonIgnore]
        public int SpecialForcesAddedToLocation => YellowForceLocations != null ? YellowForceLocations.Values.Sum(b => b.AmountOfSpecialForces) : 0;

        [JsonIgnore]
        public Dictionary<Location, Battalion> ForceLocations => YellowForceLocations;

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!Passed)
            {
                var player = Player;
                var ambassador = GetAmbassador(Game);
                var victim = GetVictim(Game);
                var victimPlayer = Game.GetPlayer(victim);

                if (ambassador == Ambassador.Blue)
                {
                    if (!GetValidBlueAmbassadors(Game).Contains(BlueSelectedAmbassador)) return Message.Express("Invalid Ambassador selected");
                    ambassador = BlueSelectedAmbassador;
                }

                switch (ambassador)
                {
                    case Ambassador.Brown:
                        if (BrownCards.Any(c => !GetValidBrownCards(player).Contains(c))) return Message.Express("Invalid card selected");
                        break;

                    case Ambassador.Pink:
                        if (PinkOfferAlliance && !AllianceCanBeOffered(Game, player)) return Message.Express("You can't offer an alliance");
                        if (PinkTakeVidal && !VidalCanBeTaken(Game)) return Message.Express("You can't take ", Game.Vidal);
                        break;

                    case Ambassador.Yellow:
                        if (YellowForceLocations.Any(kvp => Game.IsInStorm(kvp.Key))) return Message.Express("Can't move from storm");
                        if (YellowForceLocations.Any(kvp => player.ForcesIn(kvp.Key) < kvp.Value.AmountOfForces)) return Message.Express("Invalid amount of ", Player.Force);
                        if (YellowForceLocations.Any(kvp => player.SpecialForcesIn(kvp.Key) < kvp.Value.AmountOfSpecialForces)) return Message.Express("Invalid amount of ", Player.SpecialForce);
                        if (!ValidYellowTargets(Game, player).Contains(YellowOrOrangeTo)) return Message.Express("Invalid target location");
                        break;

                    case Ambassador.Grey:
                        if (!GetValidGreyCards(player).Any()) return Message.Express("You don't have a card to discard");
                        if (GreyCard != null && !GetValidGreyCards(player).Contains(GreyCard)) return Message.Express("Invalid card selected");
                        break;

                    case Ambassador.White:
                        if (player.Resources < 3) return Message.Express("You don't have enough ", Concept.Resource, " to buy a card");
                        if (!player.HasRoomForCards) return Message.Express("You don't have room for an additional card");
                        break;

                    case Ambassador.Orange:
                        if (!ValidOrangeTargets(Game, player).Contains(YellowOrOrangeTo)) return Message.Express("Invalid target location");
                        if (OrangeForceAmount > ValidOrangeMaxForces(player)) return Message.Express("Invalid amount of forces");
                        break;

                    case Ambassador.Purple:
                        if (PurpleAmountOfForces < 0) return Message.Express("You can't revive a negative amount of forces");
                        if (PurpleAmountOfForces > ValidPurpleMaxAmount(player)) return Message.Express("You can't revive that many");
                        if (PurpleAmountOfForces > 0 && PurpleHero != null) return Message.Express("You can't revive both forces and a leader");
                        if (PurpleHero != null && !ValidPurpleHeroes(Game, player).Contains(PurpleHero)) return Message.Express("Invalid leader");
                        if (PurpleAssignSkill && PurpleHero == null) return Message.Express("Select a leader to assign a skill to");
                        if (PurpleAssignSkill && !Revival.MayAssignSkill(Game, Player, PurpleHero)) return Message.Express("You can't assign a skill to this leader");
                        break;
                }
            }

            return null;
        }

        public static Territory GetTerritory(Game g) => g.LastAmbassadorTrigger?.Territory;

        public static Faction GetVictim(Game g) => g.LastAmbassadorTrigger != null ? g.LastAmbassadorTrigger.Initiator : Faction.None;

        public static Player GetVictimPlayer(Game g) => g.LastAmbassadorTrigger != null ? g.GetPlayer(GetVictim(g)) : null;

        public static Ambassador GetAmbassador(Game g) => g.LastAmbassadorTrigger != null ? g.AmbassadorIn(GetTerritory(g)) : Ambassador.None;

        public static bool AllianceCanBeOffered(Game g, Player p) => !p.HasAlly && !g.GetPlayer(GetVictim(g)).HasAlly;

        public static bool VidalCanBeTaken(Game g) => g.VidalIsAlive && !g.VidalIsCapturedOrGhola && g.OccupierOf(World.Pink) == null;

        public static IEnumerable<Ambassador> GetValidBlueAmbassadors(Game g) => g.UnassignedAmbassadors.Items;

        public static IEnumerable<TreacheryCard> GetValidBrownCards(Player p) => p.TreacheryCards;

        public static IEnumerable<TreacheryCard> GetValidGreyCards(Player p) => p.TreacheryCards;

        public static IEnumerable<Territory> ValidYellowSources(Game g, Player p) => PlacementEvent.TerritoriesWithAnyForcesNotInStorm(g, p);

        public static IEnumerable<Location> ValidYellowTargets(Game g, Player p) => g.Map.Locations(false).Where(l => l.Sector != g.SectorInStorm && l != g.Map.HiddenMobileStronghold && g.IsNotFull(p, l));

        public static IEnumerable<Location> ValidOrangeTargets(Game g, Player p)
        {
            return g.Map.Locations(g.Applicable(Rule.Homeworlds)).Where(l =>
                l.Sector != g.SectorInStorm &&
                (l != g.Map.HiddenMobileStronghold || p.Is(Faction.Grey)) &&
                !g.ContainsConflictingAlly(p, l) &&
                Shipment.IsEitherValidHomeworldOrNoHomeworld(g, p, l, false) &&
                Shipment.IsEitherValidDiscoveryOrNoDiscovery(l) &&
                g.IsNotFull(p, l));
        }

        public static int ValidOrangeMaxForces(Player p) => Math.Min(p.ForcesInReserve, 4);

        public static int ValidPurpleMaxAmount(Player p) => Math.Min(p.ForcesKilled, 4);

        public static IEnumerable<IHero> ValidPurpleHeroes(Game game, Player player) => game.KilledHeroes(player);

        public static bool MayPass(Player p) => p.Faction == Faction.Pink;

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.CurrentAmbassadorActivated = this;
            var ambassadorFaction = GetAmbassador(Game);
            var territory = GetTerritory(Game);
            var victim = GetVictim(Game);
            Game.DequeueIntrusion(IntrusionType.Ambassador);

            if (!Passed)
            {
                Game.AmbassadorsOnPlanet.Remove(territory);
                Game.Stone(Milestone.AmbassadorActivated);

                var pink = GetPlayer(Faction.Pink);

                if (ambassadorFaction == Ambassador.Blue)
                {
                    ambassadorFaction = BlueSelectedAmbassador;
                    Log("The ", Ambassador.Blue, " Ambassador is removed from the game");
                }
                else if (ambassadorFaction == Ambassador.Pink)
                {
                    pink.Ambassadors.Add(ambassadorFaction);
                    Log("The ", ambassadorFaction, " Ambassador returns to supply");
                }
                else
                {
                    Game.AmbassadorsSetAside.Add(ambassadorFaction);
                    Log("The ", ambassadorFaction, " Ambassador is set aside");
                }

                HandleAmbassador(ambassadorFaction, victim);

                if (!pink.Ambassadors.Union(Game.AmbassadorsOnPlanet.Values).Any(f => f != Ambassador.Pink))
                {
                    Game.AssignRandomAmbassadors(pink);
                    Log(Faction.Pink, " draw 5 random Ambassadors");
                }
            }
            else
            {
                Log(Initiator, " don't activate an Ambassador");
                Game.DetermineNextShipmentAndMoveSubPhase();
            }
        }

        private void HandleAmbassador(Ambassador ambassadorFaction, Faction victim)
        {
            var victimPlayer = Game.GetPlayer(victim);

            Log(Initiator, " activate the ", ambassadorFaction, " ambassador");

            switch (ambassadorFaction)
            {
                case Ambassador.Green:

                    if (victimPlayer.TreacheryCards.Any())
                    {
                        Log(Initiator, " see all treachery cards owned by ", victim);
                        LogTo(Initiator, victim, " own: ", victimPlayer.TreacheryCards);
                    }
                    else
                    {
                        Log(victim, " don't own any cards");
                    }

                    Game.DetermineNextShipmentAndMoveSubPhase();
                    break;

                case Ambassador.Brown:

                    int totalEarned = 0;
                    foreach (var c in BrownCards)
                    {
                        totalEarned += 3;
                        Game.Discard(Player, c);
                    }

                    Log(Initiator, " get ", Payment.Of(totalEarned));
                    Player.Resources += totalEarned;
                    Game.DetermineNextShipmentAndMoveSubPhase();
                    break;

                case Ambassador.Pink:

                    if (PinkOfferAlliance)
                    {
                        Game.AllianceByAmbassadorOfferedTo = victim;
                        Log(Initiator, " offer an alliance to ", victim, MessagePart.ExpressIf(PinkGiveVidalToAlly, " offering ", Game.Vidal, " if they accept"));
                        Game.PausedAmbassadorPhase = Game.CurrentPhase;
                        Game.Enter(Phase.AllianceByAmbassador);
                    }
                    else 
                    {
                        if (Game.CurrentAmbassadorActivated.PinkTakeVidal)
                        {
                            Game.TakeVidal(Player, VidalMoment.AfterUsedInBattle);
                        }

                        Game.DetermineNextShipmentAndMoveSubPhase();
                    }
                    break;

                case Ambassador.Red:

                    Player.Resources += 5;
                    Log(Initiator, " get ", Payment.Of(5));
                    Game.DetermineNextShipmentAndMoveSubPhase();
                    break;

                case Ambassador.Yellow:

                    Game.PerformMoveFromLocations(Player, YellowForceLocations, this, false, false);
                    Game.RecentMoves.Add(this);
                    Game.CheckIntrusion(this);
                    Game.DetermineNextShipmentAndMoveSubPhase();
                    break;

                case Ambassador.Black:

                    if (victim == Faction.Black)
                    {
                        Log(Initiator, " see one of the ", Faction.Black, " traitors");
                    }
                    else if (victim == Faction.Purple)
                    {
                        Log(Initiator, " see one of the ", Faction.Purple, " unrevealed Face Dancers");
                    }
                    else
                    {
                        Log(Initiator, " see the ", victim, " traitor");
                    }

                    var toSelectFrom = victim == Faction.Purple ? victimPlayer.FaceDancers.Where(t => !victimPlayer.RevealedDancers.Contains(t)) : victimPlayer.Traitors;
                    var revealed = toSelectFrom.RandomOrDefault(Game.Random);
                    LogTo(Initiator, victim, " reveal ", revealed);
                    LogTo(victim, Initiator, " get to see ", revealed);
                    Game.DetermineNextShipmentAndMoveSubPhase();
                    break;

                case Ambassador.Grey:

                    Game.Discard(Player, GreyCard);
                    Log(Initiator, " draw a new card");
                    Player.TreacheryCards.Add(Game.DrawTreacheryCard());
                    Game.DetermineNextShipmentAndMoveSubPhase();
                    break;

                case Ambassador.White:

                    Log(Initiator, " buy a card for ", Payment.Of(3));
                    Player.Resources -= 3;
                    Player.TreacheryCards.Add(Game.DrawTreacheryCard());
                    Game.DetermineNextShipmentAndMoveSubPhase();
                    break;

                case Ambassador.Orange:

                    Log(Initiator, " send ", OrangeForceAmount, Player.Force, " to ", YellowOrOrangeTo);
                    Player.ShipForces(YellowOrOrangeTo, OrangeForceAmount);
                    Game.LastShipmentOrMovement = this;
                    Game.CheckIntrusion(this);
                    Game.DetermineNextShipmentAndMoveSubPhase();
                    break;

                case Ambassador.Purple:

                    Game.DetermineNextShipmentAndMoveSubPhase();
                    if (PurpleHero != null)
                    {
                        if (!Game.IsFaceDownDead(PurpleHero))
                        {
                            Log(Initiator, " revive ", PurpleHero);
                        }
                        else
                        {
                            Log(Initiator, " revive a face down leader");
                        }

                        Game.Revive(Player, PurpleHero);

                        if (PurpleAssignSkill)
                        {
                            Game.PrepareSkillAssignmentToRevivedLeader(Player, PurpleHero as Leader);
                        }
                    }
                    else
                    {
                        Log(Initiator, " revive ", PurpleAmountOfForces, " ", Player.Force);
                        Player.ReviveForces(PurpleAmountOfForces);
                    }

                    break;
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, Passed ? " don't" : "", " activate an Ambassador");
        }

        #endregion Execution
    }
}