using System;

namespace Treachery.Shared
{
    public class LeaderState : ICloneable
    {
        private static int moment;

        public Territory CurrentTerritory { get; set; }

        //even = alive, odd = dead
        public int DeathCounter { get; set; }

        public int TimeOfDeath { get; set; }

        public LeaderSkill Skill { get; set; }

        public bool InFrontOfShield { get; set; }

        public bool Alive => (DeathCounter % 2) == 0;

        public bool IsFaceDownDead => (DeathCounter % 4) == 3;

        public void Kill(Game g)
        {
            if (Skill != LeaderSkill.None)
            {
                g.SkillDeck.PutOnTop(Skill);
                Skill = LeaderSkill.None;
                InFrontOfShield = false;
            }

            DeathCounter++;
            TimeOfDeath = moment++;
        }

        public void Assassinate(Game g)
        {
            Kill(g);
            if (!IsFaceDownDead)
            {
                Revive();
                Kill(g);
            }
        }

        public void Revive()
        {
            DeathCounter++;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
