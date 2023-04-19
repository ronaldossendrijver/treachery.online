/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
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
            if (currentOwner != null && h is Leader l && (Version >= 154 || initiator.Faction == Faction.Purple && h.Faction != Faction.Purple))
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

        public bool IsAlive(IHero l) => LeaderState[l].Alive;

        public bool IsFaceDownDead(IHero l) => LeaderState[l].IsFaceDownDead;

        public int DeathCount(IHero h) => LeaderState[h].DeathCounter;

        public bool CanFightIn(IHero h, Territory t)
        {
            var territory = LeaderState[h].CurrentTerritory;
            return territory == null || territory == t;
        }

        public bool SkilledAs(IHero leader, LeaderSkill skill) => Skill(leader) == skill;

        public bool SkilledAs(Player p, LeaderSkill skill) => p.Leaders.Any(l => Skill(l) == skill && IsInFrontOfShield(l));

        public bool IsSkilled(IHero l) => Skill(l) != LeaderSkill.None;

        public Player PlayerSkilledAs(LeaderSkill skill) => Players.FirstOrDefault(p => SkilledAs(p, skill));

        public IEnumerable<Leader> GetSkilledLeaders(Player player) => player.Leaders.Where(l => IsSkilled(l));

        public LeaderSkill GetSkill(Player p) => Skill(GetSkilledLeaders(p).FirstOrDefault(l => IsInFrontOfShield(l)));

        public LeaderSkill Skill(IHero l)
        {
            if (l != null && LeaderState.TryGetValue(l, out var state))
            {
                return state.Skill;
            }
            else
            {
                return LeaderSkill.None;
            }
        }

        internal void SetSkill(Leader l, LeaderSkill skill) => LeaderState[l].Skill = skill;

        internal void SetInFrontOfShield(Leader l, bool value)
        {
            if (l != null && LeaderState.TryGetValue(l, out LeaderState ls))
            {
                ls.InFrontOfShield = value;
            }
        }

        public bool IsInFrontOfShield(IHero l) => l != null && LeaderState.ContainsKey(l) && LeaderState[l].InFrontOfShield;

        public bool MessiahIsAlive => IsAlive(LeaderManager.Messiah);

        private bool HasSomethingToRevive(Player player)
        {
            if (player.ForcesKilled > 0 || player.SpecialForcesKilled > 0 || Revival.ValidRevivalHeroes(this, player).Any())
            {
                return true;
            }
            else if (player.Is(Faction.Purple) && player.Ally != Faction.None)
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

            if (p.Is(Faction.Green) && !IsAlive(LeaderManager.Messiah))
            {
                result.Add(LeaderManager.Messiah);
            }

            return result;
        }
    }
}
