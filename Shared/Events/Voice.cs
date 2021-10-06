/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public class Voice : GameEvent
    {
        public bool Must { get; set; }

        public TreacheryCardType Type { get; set; }

        public Voice(Game game) : base(game)
        {
        }

        public Voice()
        {
        }

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static IEnumerable<TreacheryCardType> ValidTypes(Game g)
        {
            var result = new List<TreacheryCardType>
            {
                TreacheryCardType.Mercenary,
                TreacheryCardType.Laser,
                TreacheryCardType.Poison,
                TreacheryCardType.Projectile,
                TreacheryCardType.Shield,
                TreacheryCardType.Antidote,
                TreacheryCardType.Useless
            };

            if (g.Applicable(Rule.GreyAndPurpleExpansionTreacheryCardsExceptPBandSSandAmal))
            {
                result.Add(TreacheryCardType.ArtilleryStrike);
                result.Add(TreacheryCardType.Chemistry);
                result.Add(TreacheryCardType.PoisonTooth);
                result.Add(TreacheryCardType.WeirdingWay);
            }

            if (g.Applicable(Rule.GreyAndPurpleExpansionTreacheryCardsPBandSS))
            {
                result.Add(TreacheryCardType.ProjectileAndPoison);
                result.Add(TreacheryCardType.ShieldAndAntidote);
            }

            if (g.IsPlaying(Faction.White))
            {
                result.Add(TreacheryCardType.Rockmelter);
                result.Add(TreacheryCardType.MirrorWeapon);
            }

            if (!g.Applicable(Rule.BlueVoiceMustNameSpecialCards))
            {
                result.Add(TreacheryCardType.PoisonDefense);
                result.Add(TreacheryCardType.ProjectileDefense);
            }

            return result;
        }

        public static bool MayUseVoice(Game g, Player p)
        {
            bool disableWhenPrescienceIsUsed = g.Version >= 108 && g.CurrentPrescience != null;

            if (!disableWhenPrescienceIsUsed && g.CurrentBattle != null && g.CurrentVoice == null && !g.Prevented(FactionAdvantage.BlueUsingVoice))
            {
                if (p.Faction == Faction.Blue)
                {
                    return g.CurrentBattle.IsInvolved(p);
                }
                else if (p.Ally == Faction.Blue && g.BlueAllyMayUseVoice)
                {
                    if (g.Version < 78)
                    {
                        return g.CurrentBattle.IsInvolved(p);
                    }
                    else
                    {
                        return g.CurrentBattle.IsAggressorOrDefender(p);
                    }
                }
            }

            return false;
        }

        public override Message GetMessage()
        {
            if (Must)
            {
                return new Message(Faction.Blue, "By Voice, {0} force the use of {1}.", Faction.Blue, Type);
            }
            else
            {
                return new Message(Faction.Blue, "By Voice, {0} deny the use of {1}.", Faction.Blue, Type);
            }
        }

        [JsonIgnore]
        public bool MayNot
        {
            get
            {
                return !Must;
            }
        }

        public static bool IsVoicedBy(Game g, bool asWeapon, TreacheryCardType cardType, TreacheryCardType voiced)
        {
            if (cardType == voiced)
            {
                return true;
            }
            else 
            {
                if (!g.Applicable(Rule.BlueVoiceMustNameSpecialCards))
                {
                    switch (voiced)
                    {
                        case TreacheryCardType.PoisonDefense: return cardType == TreacheryCardType.Antidote || (!asWeapon && cardType == TreacheryCardType.Chemistry) || cardType == TreacheryCardType.ShieldAndAntidote;
                        case TreacheryCardType.Poison: return cardType == TreacheryCardType.PoisonTooth || cardType == TreacheryCardType.ProjectileAndPoison;
                        case TreacheryCardType.Shield: return cardType == TreacheryCardType.ShieldAndAntidote;
                        case TreacheryCardType.ProjectileDefense: return cardType == TreacheryCardType.Shield || cardType == TreacheryCardType.ShieldAndAntidote;
                        case TreacheryCardType.Projectile: return (asWeapon && cardType == TreacheryCardType.WeirdingWay) || cardType == TreacheryCardType.ProjectileAndPoison;
                    }
                }
                else
                {
                    switch (voiced)
                    {
                        case TreacheryCardType.PoisonDefense: return cardType == TreacheryCardType.Antidote;
                        case TreacheryCardType.ProjectileDefense: return cardType == TreacheryCardType.Shield;
                    }
                }
            }

            return false;
        }
    }
}
