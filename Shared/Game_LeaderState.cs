/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Linq;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public partial class Game
    {
        private void KillHero(IHero l)
        {
            if (l is Leader || l is Messiah)
            {
                LeaderState[l].Kill(this);

                var black = GetPlayer(Faction.Black);
                if (black != null)
                {
                    ReturnCapturedLeaders(black, l);
                }

                ReturnGholaToOriginalFaction(l);
            }
        }

        private void ReturnGholaToOriginalFaction(IHero l)
        {
            var purple = GetPlayer(Faction.Purple);
            if (purple != null && l is Leader && purple.Leaders.Contains(l) && l.Faction != Faction.Purple)
            {
                var originalOwner = GetPlayer(l.Faction);
                purple.Leaders.Remove(l as Leader);
                originalOwner.Leaders.Add(l as Leader);
            }
        }

        private void AssassinateLeader(Leader l)
        {
            LeaderState[l].Assassinate(this);
        }


        public bool IsAlive(IHero l)
        {
            return LeaderState[l].Alive;
        }

        public bool SkilledAs(IHero leader, LeaderSkill skill)
        {
            return Skill(leader) == skill;
        }

        public bool SkilledAs(Player p, LeaderSkill skill)
        {
            return p.Leaders.Any(l => LeaderState[l].Skill == skill && IsInFrontOfShield(l));
        }

        public bool Skilled(IHero l)
        {
            return Skill(l) != LeaderSkill.None;
        }

        public Player PlayerSkilledAs(LeaderSkill skill)
        {
            return Players.FirstOrDefault(p => SkilledAs(p, skill));
        }

        public Leader GetSkilledLeader(Player player)
        {
            return player.Leaders.FirstOrDefault(l => Skilled(l) && !CapturedLeaders.ContainsKey(l));
        }

        public LeaderSkill Skill(Player p)
        {
            var skilledLeader = GetSkilledLeader(p);

            if (IsInFrontOfShield(skilledLeader))
            {
                return Skill(skilledLeader);
            }

            return LeaderSkill.None;
        }

        public LeaderSkill Skill(IHero l)
        {
            if (l != null && LeaderState.ContainsKey(l))
            {
                return LeaderState[l].Skill;
            }
            else
            {
                return LeaderSkill.None;
            }
        }

        private void SetSkill(Leader l, LeaderSkill skill)
        {
            LeaderState[l].Skill = skill;
        }

        private void SwitchInFrontOfShield(Leader l)
        {
            LeaderState[l].InFrontOfShield = !LeaderState[l].InFrontOfShield;
        }

        private void SetInFrontOfShield(Leader l, bool value)
        {
            LeaderState[l].InFrontOfShield = value;
        }

        public bool IsInFrontOfShield(IHero l)
        {
            return l != null && LeaderState.ContainsKey(l) && LeaderState[l].InFrontOfShield;
        }

        public bool MessiahIsAlive => IsAlive(LeaderManager.Messiah);

        public Territory CurrentTerritory(IHero l)
        {
            return LeaderState[l].CurrentTerritory;
        }

        public bool IsFaceUp(IHero l)
        {
            return !LeaderState[l].IsFaceDownDead;
        }

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
