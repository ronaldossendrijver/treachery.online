using System.Reflection;

namespace Treachery.Test;

[TestClass]
public class SecondEditionCardTests
{
    [TestMethod]
    public void VoiceTargetsCategoriesAndCannotCommandWorthlessOrCheapHeroes()
    {
        var game = CreateGame(Faction.Blue, Faction.Red);
        game.Rules.Add(Rule.SE_VoiceByCategory);
        var types = Voice.ValidTypes(game).ToArray();
        Assert.IsFalse(types.Contains(TreacheryCardType.Useless));
        Assert.IsFalse(types.Contains(TreacheryCardType.Mercenary));
        Assert.IsTrue(Voice.IsVoicedBy(game, true, true, TreacheryCardType.Laser, TreacheryCardType.Weapon));
        Assert.IsFalse(Voice.IsVoicedBy(game, true, true, TreacheryCardType.PoisonTooth, TreacheryCardType.Poison));
        Assert.IsTrue(Voice.IsVoicedBy(game, false, true, TreacheryCardType.ShieldAndAntidote, TreacheryCardType.Defense));
        Assert.IsTrue(Voice.IsVoicedBy(game, false, true, TreacheryCardType.ShieldAndAntidote, TreacheryCardType.ProjectileDefense));
        Assert.IsFalse(Voice.IsVoicedBy(game, false, true, TreacheryCardType.WeirdingWay, TreacheryCardType.ProjectileDefense));
        Assert.IsTrue(Voice.IsVoicedBy(game, false, false, TreacheryCardType.WeirdingWay, TreacheryCardType.ProjectileDefense));
    }
    [TestMethod]
    public void SpiritWorldAddsAnAlternativeWithoutReplacingKarama()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        game.Rules.Add(Rule.SpiritWorldKarama);
        var green = game.GetPlayer(Faction.Green)!;
        var karama = TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Karma);
        var atomics = TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Metheor);
        green.TreacheryCards.AddRange([karama, atomics]);
        CollectionAssert.AreEquivalent(new[] { karama, atomics }, Karma.ValidPowerCards(game, green).ToArray());
        Assert.IsFalse(Karma.ValidKarmaCards(game, green).Contains(atomics));
    }

    [TestMethod]
    public void BeneGesseritWorthlessCardsSupportEitherEffectButSolarisCannotCancel()
    {
        var game = CreateGame(Faction.Blue, Faction.Red);
        game.Rules.Add(Rule.BlueWorthlessAsKarma);
        var blue = game.GetPlayer(Faction.Blue)!;
        var worthless = TreacheryCardManager.Items.First(c => c.IsUseless);
        var solaris = TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Solaris);
        blue.TreacheryCards.AddRange([worthless, solaris]);
        Assert.IsTrue(Karma.ValidKarmaCards(game, blue).Contains(worthless));
        Assert.IsTrue(Karma.ValidSolarisCards(game, blue).Contains(worthless));
        Assert.IsFalse(Karma.ValidKarmaCards(game, blue).Contains(solaris));
    }

    [TestMethod]
    public void SolarisCanBuyAnAuctionCardOrDiscountAReserveShipmentButCannotRemoveBidLimits()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        game.Rules.Add(Rule.SE_Solaris);
        Set(game, nameof(Game.CurrentAuctionType), AuctionType.Normal);
        var green = game.GetPlayer(Faction.Green)!;
        var solaris = TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Solaris);
        green.TreacheryCards.Add(solaris);

        Assert.IsTrue(Bid.ValidKarmaCards(game, green).Contains(solaris));
        Assert.IsNull(new Bid(game, Faction.Green) { KarmaBid = true, KarmaCard = solaris }.Validate());
        Assert.IsNotNull(new Bid(game, Faction.Green) { KarmaBid = true }.Validate());
        Assert.IsNotNull(new Bid(game, Faction.Green) { Amount = 101, KarmaCard = solaris }.Validate());
        Assert.IsTrue(Shipment.ValidKarmaCards(game, green).Contains(solaris));

        var shipment = new Shipment(game, Faction.Green)
        {
            To = game.Map.Arrakeen,
            ForceAmount = 1,
            KarmaCard = solaris
        };
        Assert.AreEqual(1, Shipment.DetermineCost(game, green, shipment));
    }

    [TestMethod]
    public void PrescienceWaitsForKaramaResponsesBeforePrivateInformationIsVisible()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        var karama = TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Karma);
        game.GetPlayer(Faction.Red)!.TreacheryCards.Add(karama);
        Set(game, nameof(Game.CurrentPhase), Phase.BattlePhase);
        Set(game, nameof(Game.CurrentPrescience), new Prescience(game, Faction.Green) { Aspect = PrescienceAspect.Weapon });
        Invoke(game, "BeginSecondEditionKaramaResponse", FactionAdvantage.GreenBattlePlanPrescience, Faction.Green);
        Assert.AreEqual(Phase.KaramaResponse, game.CurrentPhase);
        Assert.IsFalse(game.MaySeeBattlePrescience(Faction.Green));
        var cancellation = new Karma(game, Faction.Red) { Card = karama, Prevented = FactionAdvantage.GreenBattlePlanPrescience };
        Assert.IsNull(cancellation.Validate());
        Assert.IsNull(cancellation.Execute(false, true));
        Assert.IsNull(game.CurrentPrescience);
        Assert.AreEqual(Phase.BattlePhase, game.CurrentPhase);
    }

    [TestMethod]
    public void SardaukarCancellationMustPrecedeEitherPlanAndCannotTargetEconomy()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        Set(game, nameof(Game.CurrentMainPhase), MainPhase.Battle);
        Set(game, nameof(Game.CurrentBattle), new BattleInitiated(game, Faction.Green) { Target = Faction.Red });
        var green = game.GetPlayer(Faction.Green)!;
        Assert.IsTrue(Karma.ValidFactionAdvantages(game, green).Contains(FactionAdvantage.RedSpecialForceBonus));
        Assert.IsFalse(Karma.ValidFactionAdvantages(game, green).Contains(FactionAdvantage.RedReceiveBid));
        Set(game, nameof(Game.DefenderPlan), new Battle(game, Faction.Red));
        Assert.IsFalse(Karma.ValidFactionAdvantages(game, green).Contains(FactionAdvantage.RedSpecialForceBonus));
    }

    [TestMethod]
    public void VendettaSkipsEmptyPlayersInBothDirections()
    {
        var game = CreateGame(Faction.Black, Faction.Green, Faction.Red);
        game.Rules.Add(Rule.SE_HarkonnenVendetta);
        var black = game.GetPlayer(Faction.Black)!;
        var green = game.GetPlayer(Faction.Green)!;
        var karama = TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Karma);
        var weapon = TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Projectile);
        var defense = TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Shield);
        black.TreacheryCards.Add(karama);
        green.TreacheryCards.Add(weapon);
        Set(game, nameof(Game.TreacheryDeck), new Deck<TreacheryCard>([defense], new LoggedRandom()));
        Set(game, nameof(Game.CurrentPhase), Phase.BiddingReport);
        var vendetta = new VendettaStarted(game, Faction.Black);
        Assert.IsNull(vendetta.Validate());
        Assert.IsNull(vendetta.Execute(false, true));
        CollectionAssert.AreEqual(new[] { Faction.Green }, game.VendettaRecipients.ToArray());
        Assert.IsNull(new VendettaCardReturned(game, Faction.Black) { CardId = TreacheryCardManager.Lookup.GetId(defense) }.Execute(false, true));
        Assert.AreEqual(0, game.GetPlayer(Faction.Red)!.TreacheryCards.Count);
        Assert.AreEqual(1, green.TreacheryCards.Count);
        Assert.AreEqual(Phase.BiddingReport, game.CurrentPhase);
        Assert.IsTrue(black.SpecialKarmaPowerUsed);
    }

    [TestMethod]
    public void VendettaReshufflesNormalDiscardWhenDeckIsEmpty()
    {
        var game = CreateGame(Faction.Black, Faction.Green);
        game.Rules.Add(Rule.SE_HarkonnenVendetta);
        var black = game.GetPlayer(Faction.Black)!;
        var karama = TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Karma);
        black.TreacheryCards.Add(karama);
        Set(game, nameof(Game.TreacheryDeck), new Deck<TreacheryCard>(new LoggedRandom()));
        Set(game, nameof(Game.CurrentPhase), Phase.BiddingReport);
        Assert.IsNull(new VendettaStarted(game, Faction.Black).Execute(false, true));
        Assert.AreEqual(1, black.TreacheryCards.Count);
        Assert.AreEqual(0, game.VendettaRecipients.Count);
        Assert.AreEqual(Phase.BiddingReport, game.CurrentPhase);
    }

    [TestMethod]
    public void AlliedSecondEditionDealCannotTransferSpice()
    {
        var game = CreateGame(Faction.Green, Faction.Black);
        game.Rules.Add(Rule.SE_Deals);
        game.GetPlayer(Faction.Green)!.Ally = Faction.Black;
        game.GetPlayer(Faction.Black)!.Ally = Faction.Green;
        var offer = new DealOffered(game, Faction.Green)
        {
            To = [Faction.Black],
            Price = 1
        };
        game.DealOffers.Add(offer);

        Assert.IsNotNull(offer.Acceptance(Faction.Black).Validate());
    }

    [TestMethod]
    public void AlliedSecondEditionDealDoesNotCreateBindingCommitment()
    {
        var game = CreateGame(Faction.Green, Faction.Black);
        game.Rules.Add(Rule.SE_Deals);
        game.GetPlayer(Faction.Green)!.Ally = Faction.Black;
        game.GetPlayer(Faction.Black)!.Ally = Faction.Green;
        game.DealOffers.Add(new DealOffered(game, Faction.Green)
        {
            To = [Faction.Black],
            Type = DealType.DontShipOrMoveTo
        });
        var accepted = game.DealOffers[0].Acceptance(Faction.Black);

        typeof(DealAccepted).GetMethod("ExecuteConcreteEvent", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(accepted, null);

        Assert.AreEqual(0, game.Deals.Count);
    }

    [TestMethod]
    public void EmperorCanGiftSpiceToAnAllyOnlyAtPhaseEnd()
    {
        var game = CreateGame(Faction.Red, Faction.Green);
        game.Rules.Add(Rule.SE_EmperorAlliance);
        game.GetPlayer(Faction.Red)!.Ally = Faction.Green;
        game.GetPlayer(Faction.Green)!.Ally = Faction.Red;
        var gift = new Donated(game, Faction.Red) { Target = Faction.Green, Resources = 1 };

        Set(game, nameof(Game.CurrentMoment), MainPhaseMoment.Middle);
        Assert.IsNotNull(gift.Validate());
        Set(game, nameof(Game.CurrentMoment), MainPhaseMoment.End);
        Assert.IsNull(gift.Validate());
    }

    [TestMethod]
    public void MentatPauseRejectsNewDealsAndBribes()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        game.Rules.Add(Rule.SE_Deals);
        game.Rules.Add(Rule.SE_CardTiming);
        Set(game, nameof(Game.CurrentMainPhase), MainPhase.Contemplate);
        var offer = new DealOffered(game, Faction.Green) { To = [Faction.Red] };
        game.DealOffers.Add(offer);

        Assert.IsNotNull(offer.Validate());
        Assert.IsNotNull(offer.Acceptance(Faction.Red).Validate());
        Assert.IsNotNull(new Donated(game, Faction.Green) { Target = Faction.Red, Resources = 1 }.Validate());
    }

    [TestMethod]
    public void WarMachineAllowsFourStarsAndOneLeaderOutsideNormalLimits()
    {
        var game = CreateGame(Faction.Red, Faction.Green);
        var red = game.GetPlayer(Faction.Red)!;
        red.SpecialForcesKilled = 4;
        red.TreacheryCards.Add(TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Karma));
        var hero = red.Leaders.First();
        game.LeaderState[hero].DeathCounter = 1;
        game.LeaderRevivalsThisTurn[Faction.Red] = 1;
        game.FactionsThatRevivedSpecialForcesThisTurn.Add(Faction.Red);
        Set(game, nameof(Game.CurrentMainPhase), MainPhase.Resurrection);
        Assert.IsNull(new KarmaFreeRevival(game, Faction.Red) { AmountOfSpecialForces = 4, Hero = hero }.Validate());
        Assert.AreEqual(4, KarmaFreeRevival.ValidMaxAmount(game, red, true));
    }

    [TestMethod]
    public void CheapHeroMayBeWithheldOnlyIfNoActualLeaderIsAvailable()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        Set(game, nameof(Game.CurrentBattle), new BattleInitiated(game, Faction.Green) { Target = Faction.Red, Territory = game.Map.Arrakeen.Territory });
        var green = game.GetPlayer(Faction.Green)!;
        green.TreacheryCards.Add(TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.Mercenary));
        Assert.IsNotNull(new Battle(game, Faction.Green) { Hero = null, Weapon = null, Defense = null }.Validate());
        foreach (var leader in green.Leaders) game.LeaderState[leader].DeathCounter = 1;
        Assert.IsNull(new Battle(game, Faction.Green) { Hero = null, Weapon = null, Defense = null }.Validate());
        Assert.IsNotNull(new Battle(game, Faction.Green) { Hero = null, Defense = null, Weapon = TreacheryCardManager.Items.First(c => c.IsWeapon) }.Validate());
    }

    private static Game CreateGame(params Faction[] factions)
    {
        var game = new Game();
        game.Rules.AddRange([Rule.SE_Core, Rule.SE_Karama, Rule.SE_SpiceBonus, Rule.SE_LeaderRevival]);
        var random = new LoggedRandom();
        Set(game, "Random", random);
        Set(game, nameof(Game.TreacheryDiscardPile), new Deck<TreacheryCard>(random));
        foreach (var faction in factions)
        {
            var player = new Player(game, faction) { Resources = 100 };
            player.AssignLeaders(game);
            game.Players.Add(player);
        }
        return game;
    }

    private static void Set(Game game, string name, object value) =>
        typeof(Game).GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(game, value);

    private static void Invoke(Game game, string name, params object?[] values) =>
        typeof(Game).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(game, values);
}
