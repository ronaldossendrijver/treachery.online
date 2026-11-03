namespace Treachery.Shared;

public partial class Game
{
    internal bool SecondEditionNexusPending { get; set; }
    internal bool SecondEditionShaiSeen { get; set; }
    internal int SecondEditionPendingWorms { get; set; }
    public List<Territory> SecondEditionRideTerritories { get; } = [];
    internal bool SecondEditionYellowRideDone { get; set; }
    internal bool SecondEditionAllyRideDone { get; set; }
    internal ResourceCard? CurrentLittleMaker { get; set; }
    public Player LastPlayerInStormOrder => new PlayerSequence(this).GetPlayersInSequence().Last().Player;

    internal void DrawSecondEditionResourceCard()
    {
        while (true)
        {
            var card = DrawAndDiscardResourceCard(ResourceCardDiscardPileA!);
            if (card.IsSpiceBlow)
            {
                LatestSpiceCardA = card;
                ProcessBlowCard(card);
                return;
            }
            if (CurrentTurn == 1)
            {
                IgnoredMonsters.Add(ResourceCardDiscardPileA!.Draw());
                Log(card, " in round 1 was ignored");
                continue;
            }
            if (card.IsLittleMaker)
            {
                CurrentLittleMaker = card;
                card.Location = null;
                SecondEditionNexusPending = true;
                if (LittleMakerPlaced.ValidTargets(this).Any()) Enter(Phase.LittleMakerPlacement);
                else
                {
                    Log("There is no vacant Spice Blow marker outside the storm; Little Maker places no spice");
                    LatestSpiceCardA = new ResourceCard(card.SkinId) { IsLittleMaker = true };
                    MoveToNextPhaseAfterResourceBlow();
                }
                return;
            }
            SecondEditionNexusPending = true;
            NumberOfMonsters++;
            if (!SecondEditionShaiSeen)
                DevourSecondEditionSpiceTerritories();
            else if (IsPlaying(Faction.Yellow) && Applicable(Rule.SE_FremenWorms))
            {
                SecondEditionPendingWorms++;
                Log("A subsequent ", Concept.Monster, " may be placed by ", Faction.Yellow, " after the Nexus");
            }
            else Log("A subsequent ", Concept.Monster, " is discarded without effect");
            SecondEditionShaiSeen = true;
        }
    }

    internal void PlaceLittleMaker(Location target)
    {
        CurrentLittleMaker!.Location = target;
        LatestSpiceCardA = new ResourceCard(CurrentLittleMaker.SkinId) { IsLittleMaker = true, Location = target };
        ChangeResourcesOnPlanet(target, 10);
        Log("Little Maker places ", Payment.Of(10), " in ", target);
        MoveToNextPhaseAfterResourceBlow();
    }

    internal void DevourSecondEditionSpiceTerritories()
    {
        foreach (var territory in (LatestSpiceCardA?.BlowLocations ?? []).Select(l => l.Territory).Distinct())
        {
            var monster = new MonsterAppearence(territory, false);
            Monsters.Add(monster);
            SecondEditionRideTerritories.Add(territory);
            PerformMonster(monster);
        }
        SecondEditionYellowRideDone = false;
        SecondEditionAllyRideDone = false;
    }

    internal void CompleteSecondEditionNexus()
    {
        if (!SecondEditionYellowRideDone && YellowRidesMonster.IsApplicable(this))
        {
            Enter(Phase.YellowRidingMonsterA);
            return;
        }
        SecondEditionYellowRideDone = true;
        var ally = GetPlayer(GetPlayer(Faction.Yellow)?.Ally ?? Faction.None);
        if (!SecondEditionAllyRideDone && YellowAllowsWormRide && ally != null &&
            AllyRidesMonster.ValidSources(this, ally).Any())
        {
            Enter(Phase.AllyRidingMonster);
            return;
        }
        SecondEditionAllyRideDone = true;
        SecondEditionRideTerritories.Clear();
        Monsters.Clear();
        if (SecondEditionPendingWorms > 0)
        {
            Enter(Phase.SecondEditionWormPlacement);
            return;
        }
        StartNexusCardPhase();
    }

    internal void PlaceSecondEditionSandworm(Territory territory)
    {
        SecondEditionPendingWorms--;
        var monster = new MonsterAppearence(territory, false);
        Monsters.Add(monster);
        SecondEditionRideTerritories.Add(territory);
        PerformMonster(monster);
        SecondEditionYellowRideDone = false;
        SecondEditionAllyRideDone = false;
        CompleteSecondEditionNexus();
    }

    internal void CallSecondEditionSandworm(bool useLastSpiceTerritories)
    {
        NumberOfMonsters++;
        SecondEditionNexusPending = true;
        SecondEditionShaiSeen = true;
        if (useLastSpiceTerritories) DevourSecondEditionSpiceTerritories();
        else SecondEditionPendingWorms++;
        if (CurrentPhase == Phase.BlowReport) Enter(Phase.AllianceA);
    }
}
