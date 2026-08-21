/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public partial class Game
{
    public Dictionary<IHero, LeaderState> LeaderState { get; private set; } = new();

    internal void KillHero(IHero h)
    {
        if (h is not (Leader or Messiah)) return;
        
        LeaderState[h].Kill(this);
        Stone(Milestone.LeaderKilled);
        DetermineIfCapturedLeadersMustBeReleased();
        DetermineIfKilledGholaReturnsToOriginalFaction(h);

        if (h.HeroType != HeroType.Vidal) return;
        var currentOwner = OwnerOf(h);
        var leaderToRemove = (Leader)h;
        currentOwner!.Leaders.Remove(leaderToRemove);
        var pink = GetPlayer(Faction.Pink);
        pink?.Leaders.Add(leaderToRemove);
    }

    internal void Revive(Player initiator, IHero h)
    {
        LeaderState[h].Revive();
        LeaderState[h].CurrentTerritory = null;

        var currentOwner = OwnerOf(h);
        if (currentOwner != null && h is Leader l && (Version >= 154 || (initiator.Faction == Faction.Purple && h.Faction != Faction.Purple)))
        {
            currentOwner.Leaders.Remove(l);
            initiator.Leaders.Add(l);
        }
    }

    private void DetermineIfKilledGholaReturnsToOriginalFaction(IHero l)
    {
        var purple = GetPlayer(Faction.Purple);
        if (purple != null && l is Leader leader && purple.Leaders.Contains(l) && l.Faction != Faction.Purple)
        {
            purple.Leaders.Remove(leader);

            GetPlayer(l.Faction)?.Leaders.Add(leader);
        }
    }

    internal void AssassinateLeader(Leader l)
    {
        LeaderState[l].Assassinate(this);
        Stone(Milestone.LeaderKilled);

        if (Version >= 150)
        {
            DetermineIfCapturedLeadersMustBeReleased();
            DetermineIfKilledGholaReturnsToOriginalFaction(l);
        }
    }

    public bool IsAlive(IHero l)
    {
        return LeaderState[l].Alive;
    }

    public bool IsFaceDownDead(IHero l)
    {
        return LeaderState[l].IsFaceDownDead;
    }

    public int DeathCount(IHero h)
    {
        return LeaderState[h].DeathCounter;
    }

    public bool CanFightIn(IHero h, Territory t)
    {
        var territory = LeaderState[h].CurrentTerritory;
        return territory == null || territory == t;
    }

    public bool SkilledAs(IHero leader, LeaderSkill skill)
    {
        return Skill(leader) == skill;
    }

    public bool SkilledAs(Player p, LeaderSkill skill)
    {
        return p.Leaders.Any(l => Skill(l) == skill && IsInFrontOfShield(l));
    }

    public bool IsSkilled(IHero? l)
    {
        return l != null && Skill(l) != LeaderSkill.None;
    }

    public Player? PlayerSkilledAs(LeaderSkill skill)
    {
        return Players.FirstOrDefault(p => SkilledAs(p, skill));
    }

    public IEnumerable<Leader> GetSkilledLeaders(Player player)
    {
        return player.Leaders.Where(IsSkilled);
    }

    public LeaderSkill? GetSkill(Player p)
    {
        var hero = GetSkilledLeaders(p).FirstOrDefault(IsInFrontOfShield);
        return hero != null ? Skill(hero) : null;
    }

    public LeaderSkill Skill(IHero? l)
    {
        if (l is null) return LeaderSkill.None;
        
        return LeaderState.TryGetValue(l, out var state) 
            ? state.Skill 
            : LeaderSkill.None;
    }

    internal void SetSkill(Leader l, LeaderSkill skill)
    {
        LeaderState[l].Skill = skill;
    }

    internal void SetInFrontOfShield(Leader l, bool value)
    {
        if (LeaderState.TryGetValue(l, out var ls)) ls.InFrontOfShield = value;
    }

    public bool IsInFrontOfShield(IHero? l)
    {
        if (l is null) return false;
        return LeaderState.ContainsKey(l) && LeaderState[l].InFrontOfShield;
    }

    public bool MessiahIsAlive => IsAlive(LeaderManager.Messiah);

    private bool HasSomethingToRevive(Player player)
    {
        while (true)
        {
            if (player.ForcesKilled > 0 || player.SpecialForcesKilled > 0 || Revival.ValidRevivalHeroes(this, player).Any())
            {
                return true;
            }

            var ally = GetPlayer(player.Ally);
            if (!player.Is(Faction.Purple) || ally == null || player.Ally == Faction.None) return false;
            player = ally;
        }
    }

    public IEnumerable<IHero> KilledHeroes(Player p)
    {
        var result = new List<IHero>();
        result.AddRange(p.Leaders.Where(l => !IsAlive(l)));

        if (p.Is(Faction.Green) && !IsAlive(LeaderManager.Messiah)) result.Add(LeaderManager.Messiah);

        return result;
    }
}