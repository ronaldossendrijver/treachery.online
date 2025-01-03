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
        if (h is Leader || h is Messiah)
        {
            LeaderState[h].Kill(this);
            Stone(Milestone.LeaderKilled);
            DetermineIfCapturedLeadersMustBeReleased();
            DetermineIfKilledGholaReturnsToOriginalFaction(h);

            if (h.HeroType == HeroType.Vidal)
            {
                var currentOwner = OwnerOf(h);
                currentOwner.Leaders.Remove(h as Leader);
                var pink = GetPlayer(Faction.Pink);
                pink?.Leaders.Add(h as Leader);
            }
        }
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
        if (purple != null && l is Leader && purple.Leaders.Contains(l) && l.Faction != Faction.Purple)
        {
            purple.Leaders.Remove(l as Leader);

            GetPlayer(l.Faction)?.Leaders.Add(l as Leader);
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

    public bool IsSkilled(IHero l)
    {
        return Skill(l) != LeaderSkill.None;
    }

    public Player PlayerSkilledAs(LeaderSkill skill)
    {
        return Players.FirstOrDefault(p => SkilledAs(p, skill));
    }

    public IEnumerable<Leader> GetSkilledLeaders(Player player)
    {
        return player.Leaders.Where(l => IsSkilled(l));
    }

    public LeaderSkill GetSkill(Player p)
    {
        return Skill(GetSkilledLeaders(p).FirstOrDefault(l => IsInFrontOfShield(l)));
    }

    public LeaderSkill Skill(IHero l)
    {
        if (l != null && LeaderState.TryGetValue(l, out var state))
            return state.Skill;
        return LeaderSkill.None;
    }

    internal void SetSkill(Leader l, LeaderSkill skill)
    {
        LeaderState[l].Skill = skill;
    }

    internal void SetInFrontOfShield(Leader l, bool value)
    {
        if (l != null && LeaderState.TryGetValue(l, out var ls)) ls.InFrontOfShield = value;
    }

    public bool IsInFrontOfShield(IHero l)
    {
        return l != null && LeaderState.ContainsKey(l) && LeaderState[l].InFrontOfShield;
    }

    public bool MessiahIsAlive => IsAlive(LeaderManager.Messiah);

    private bool HasSomethingToRevive(Player player)
    {
        if (player.ForcesKilled > 0 || player.SpecialForcesKilled > 0 || Revival.ValidRevivalHeroes(this, player).Any())
        {
            return true;
        }

        if (player.Is(Faction.Purple) && player.Ally != Faction.None)
        {
            var ally = GetPlayer(player.Ally);
            return HasSomethingToRevive(ally);
        }

        return false;
    }

    public IEnumerable<IHero> KilledHeroes(Player p)
    {
        var result = new List<IHero>();
        result.AddRange(p.Leaders.Where(l => !IsAlive(l)));

        if (p.Is(Faction.Green) && !IsAlive(LeaderManager.Messiah)) result.Add(LeaderManager.Messiah);

        return result;
    }
}