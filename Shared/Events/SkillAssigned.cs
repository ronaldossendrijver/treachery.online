/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class SkillAssigned : PassableGameEvent
{
    #region Construction

    public SkillAssigned(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public SkillAssigned()
    {
    }

    #endregion Construction

    #region Properties

    public LeaderSkill Skill;

    public int _leaderId;

    [JsonIgnore]
    public Leader Leader
    {
        get => LeaderManager.LeaderLookup.Find(_leaderId);
        set => _leaderId = LeaderManager.LeaderLookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Passed && Game.CurrentPhase == Phase.AssigningInitialSkills) return Message.Express("You must assign a leader skill");

        return null;
    }

    public static bool PlayersMustChooseLeaderSkills(Game g) => g.Players.Any(p => p.SkillsToChooseFrom.Count != 0);

    public static IEnumerable<LeaderSkill> ValidSkills(Player p)
    {
        return p.SkillsToChooseFrom;
    }

    public static IEnumerable<Leader> ValidLeaders(Game g, Player p)
    {
        if (g.CurrentPhase == Phase.AssigningInitialSkills)
            return p.Leaders.Where(l => l.HeroType != HeroType.Auditor);
        return new[] { p.MostRecentlyRevivedLeader };
    }

    #endregion Validation

    #region Execution
    
    protected override void ExecuteConcreteEvent()
    {
        Log();
        Game.SetSkill(Leader, Skill);
        Player.SkillsToChooseFrom.Remove(Skill);
        Game.SetInFrontOfShield(Leader, true);
        Game.SkillDeck.PutOnTop(Player.SkillsToChooseFrom);
        Player.SkillsToChooseFrom.Clear();

        if (!PlayersMustChooseLeaderSkills(Game))
        {
            Game.SkillDeck.Shuffle();
            Game.Enter(Game.CurrentPhase != Phase.AssigningInitialSkills, Game.PhaseBeforeSkillAssignment, Game.TreacheryCardsBeforeTraitors, Game.DealTraitors, Game.SetupSpiceAndForces);
        }
    }

    public override Message GetMessage()
    {
        return Message.Express(Leader, " becomes ", Skill);
    }


    #endregion Execution
}