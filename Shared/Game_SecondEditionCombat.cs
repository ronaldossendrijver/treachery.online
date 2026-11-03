namespace Treachery.Shared;

public partial class Game
{
    public Faction GoldenPathTarget { get; internal set; }

    public bool MaySeeGoldenPathPlan(Faction observer, Battle plan) =>
        GreenKarma && (observer == Faction.Green || GetAlly(observer) == Faction.Green) &&
        (!SecondEdition || plan.Initiator == GoldenPathTarget);

    public bool MaySeeBattlePrescience(Faction observer) => CurrentPhase != Phase.KaramaResponse &&
        CurrentPrescience != null && (observer == Faction.Green || GetAlly(observer) == Faction.Green);
    public FactionAdvantage PendingKaramaAdvantage { get; private set; }
    public List<Faction> KaramaResponders { get; } = [];
    private Phase PhaseBeforeKaramaResponse { get; set; }

    internal void BeginSecondEditionKaramaResponse(FactionAdvantage advantage, Faction initiator)
    {
        if (!Applicable(Rule.SE_Karama)) return;
        var owner = advantage == FactionAdvantage.GreenBattlePlanPrescience ? Faction.Green :
            advantage == FactionAdvantage.BlueUsingVoice ? Faction.Blue : initiator;
        KaramaResponders.Clear();
        KaramaResponders.AddRange(Players.Where(p => p.Faction != initiator && p.Faction != owner && Karma.CanKarma(this, p)).Select(p => p.Faction));
        if (KaramaResponders.Count == 0) return;
        PendingKaramaAdvantage = advantage;
        PhaseBeforeKaramaResponse = CurrentPhase;
        Enter(Phase.KaramaResponse, false);
    }

    public void FinishSecondEditionKaramaResponse()
    {
        if (CurrentPhase != Phase.KaramaResponse) return;
        var advantage = PendingKaramaAdvantage;
        PendingKaramaAdvantage = FactionAdvantage.None;
        KaramaResponders.Clear();
        Enter(PhaseBeforeKaramaResponse, false);
        if (advantage == FactionAdvantage.BlueUsingVoice && CurrentVoice != null && CurrentBattle != null)
        {
            var opponent = CurrentBattle.OpponentOf(CurrentVoice.Player);
            if (opponent != null) RevokePlanIfNeeded(opponent.Faction);
        }
    }

    internal void RevealSecondEditionBattlePayments()
    {
        if (AggressorPlan == null || DefenderPlan == null) return;
        foreach (var plan in new[] { AggressorPlan, DefenderPlan })
        {
            if (plan.SecondEditionPaymentRevealed) continue;
            plan.SecondEditionPaymentRevealed = true;
            if (Applicable(Rule.SE_BattleResolution) || Applicable(Rule.SE_SpiceBonus))
            {
                plan.SecondEditionSpicePaid = plan.Cost(this) - plan.AllyContributionAmount;
                plan.Player.Resources -= plan.SecondEditionSpicePaid;
                if (plan.AllyContributionAmount > 0 && plan.Player.AlliedPlayer is { } ally)
                {
                    plan.SecondEditionAllySpicePaid = plan.AllyContributionAmount;
                    ally.Resources -= plan.AllyContributionAmount;
                    DecreasePermittedUseOfAllySpice(plan.Initiator, plan.AllyContributionAmount);
                }
                if (plan.SolarisCard != null) Discard(plan.Player, plan.SolarisCard);
                LogIf(plan.SecondEditionSpicePaid > 0, plan.Initiator, " pay ", Payment.Of(plan.SecondEditionSpicePaid), " for spice strength");
            }
            if (Applicable(Rule.SE_BattleResolution))
                foreach (var card in new[] { plan.Weapon, plan.Defense }.Where(c => c is { IsUseless: true }).Distinct())
                    Discard(plan.Player, card);
        }
    }
}
