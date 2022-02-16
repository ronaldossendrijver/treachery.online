namespace Treachery.Shared
{
    public class BattleOutcome
    {
        public int AggHeroSkillBonus;
        public LeaderSkill AggActivatedBonusSkill;
        public int DefHeroSkillBonus;
        public LeaderSkill DefActivatedBonusSkill;
        public int AggBattlePenalty;
        public LeaderSkill AggActivatedPenaltySkill;
        public int DefBattlePenalty;
        public int AggMessiahContribution;
        public LeaderSkill DefActivatedPenaltySkill;
        public int DefMessiahContribution;
        public Player Winner;
        public Player Loser;
        public bool AggHeroKilled;
        public bool LoserHeroKilled => Loser == Aggressor ? AggHeroKilled : DefHeroKilled;
        public TreacheryCardType AggHeroCauseOfDeath;
        public int AggHeroEffectiveStrength;
        public bool DefHeroKilled;
        public bool WinnerHeroKilled => Winner == Aggressor ? AggHeroKilled : DefHeroKilled;
        public TreacheryCardType DefHeroCauseOfDeath;
        public int DefHeroEffectiveStrength;
        public float AggTotal;
        public float DefTotal;
        public Battle WinnerBattlePlan;
        public Battle LoserBattlePlan;

        public bool AggSavedByCarthag;
        public bool DefSavedByCarthag;

        public Player Aggressor;
        public Player Defender;

        

        public override string ToString()
        {
            return Skin.Current.Format("Aggressor ({0}) {1}. Total strength: {2}, leader {3} by {4}. Defender ({5}) {6}. Total strength: {7}, leader {8} by {9}.",

                Aggressor.Faction,
                Winner == Aggressor ? "win" : "lose",
                AggTotal,
                AggHeroKilled ? "killed" : "survives",
                AggHeroCauseOfDeath,

                Defender.Faction,
                Winner == Defender ? "win" : "lose",
                DefTotal,
                DefHeroKilled ? "killed" : "survives",
                DefHeroCauseOfDeath
                );
        }
    }
}
