/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
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
