/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

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
        public LeaderSkill DefActivatedPenaltySkill;

        public int AggMessiahContribution;
        public int DefMessiahContribution;

        public int AggHomeworldContribution;
        public int DefHomeworldContribution;

        public int AggReinforcementsContribution;
        public int DefReinforcementsContribution;

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

        public int AggUndialedForces;
        public int DefUndialedForces;

        public float AggTotal;
        public float DefTotal;

        public Battle WinnerBattlePlan;
        public Battle LoserBattlePlan;

        public bool AggSavedByCarthag;
        public bool DefSavedByCarthag;

        public Player Aggressor;
        public Player Defender;



        public Message GetMessage()
        {
            return Message.Express(
                "Aggressor (",
                Aggressor.Faction,
                ") ",
                Winner == Aggressor ? "win" : "lose",
                ". Total strength: ",
                AggTotal,
                ", leader ",
                AggHeroKilled ? "killed" : "survives",
                " by ",
                AggHeroCauseOfDeath,
                ". Defender (",
                Defender.Faction,
                ") ",
                Winner == Defender ? "win" : "lose",
                ". Total strength: ",
                DefTotal,
                ", leader ",
                DefHeroKilled ? "killed" : "survives",
                " by ",
                DefHeroCauseOfDeath,
                ".");
        }
    }
}
