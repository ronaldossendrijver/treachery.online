/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class ClairVoyanceAnswered : GameEvent
    {
        #region Construction

        public ClairVoyanceAnswered(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public ClairVoyanceAnswered()
        {
        }

        #endregion Construction

        #region Properties

        public ClairVoyanceAnswer Answer { get; set; }

        [JsonIgnore]
        public bool IsNo => Answer == ClairVoyanceAnswer.No;

        [JsonIgnore]
        public bool IsYes => Answer == ClairVoyanceAnswer.Yes;

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool IsQuestionedBy(bool asWeapon, TreacheryCardType cardType, TreacheryCardType asked)
        {
            if (cardType == asked)
            {
                return true;
            }
            else
            {
                switch (asked)
                {
                    case TreacheryCardType.PoisonDefense: return cardType == TreacheryCardType.Antidote || (!asWeapon && cardType == TreacheryCardType.Chemistry) || cardType == TreacheryCardType.ShieldAndAntidote;
                    case TreacheryCardType.Poison: return cardType == TreacheryCardType.PoisonTooth || cardType == TreacheryCardType.ProjectileAndPoison;
                    case TreacheryCardType.Shield: return cardType == TreacheryCardType.ShieldAndAntidote;
                    case TreacheryCardType.ProjectileDefense: return cardType == TreacheryCardType.Shield || cardType == TreacheryCardType.ShieldAndAntidote;
                    case TreacheryCardType.Projectile: return (asWeapon && cardType == TreacheryCardType.WeirdingWay) || cardType == TreacheryCardType.ProjectileAndPoison;
                }
            }

            return false;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.LatestClairvoyanceQandA = new ClairVoyanceQandA(Game.LatestClairvoyance, this);

            Log();

            if (Game.LatestClairvoyance.Question == ClairvoyanceQuestion.WillAttackX && IsNo)
            {
                var deal = new Deal()
                {
                    Type = DealType.DontShipOrMoveTo,
                    BoundFaction = Initiator,
                    ConsumingFaction = Game.LatestClairvoyance.Initiator,
                    DealParameter1 = Game.LatestClairvoyance.QuestionParameter1,
                    End = Phase.ShipmentAndMoveConcluded
                };

                Game.StartDeal(deal);
            }
            else if (Game.LatestClairvoyance.Question == ClairvoyanceQuestion.LeaderAsTraitor)
            {
                var hero = Game.LatestClairvoyance.Parameter1 as IHero;

                if (IsYes)
                {
                    if (!Player.ToldTraitors.Contains(hero))
                    {
                        Player.ToldTraitors.Add(hero);
                    }
                }
                else if (IsNo)
                {
                    if (!Player.ToldNonTraitors.Contains(hero))
                    {
                        Player.ToldNonTraitors.Add(hero);
                    }
                }
            }
            else if (Game.LatestClairvoyance.Question == ClairvoyanceQuestion.LeaderAsFacedancer)
            {
                var hero = Game.LatestClairvoyance.Parameter1 as IHero;

                if (Answer == ClairVoyanceAnswer.Yes)
                {
                    if (!Player.ToldFacedancers.Contains(hero))
                    {
                        Player.ToldFacedancers.Add(hero);
                    }
                }
                else if (Answer == ClairVoyanceAnswer.No)
                {
                    if (!Player.ToldNonFacedancers.Contains(hero))
                    {
                        Player.ToldNonFacedancers.Add(hero);
                    }
                }
            }

            Game.Enter(Game.PhasePausedByClairvoyance, false);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " answer: ", Answer);
        }

        #endregion Execution
    }
}
