/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

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
            return true;
        switch (asked)
        {
            case TreacheryCardType.PoisonDefense: return cardType == TreacheryCardType.Antidote || (!asWeapon && cardType == TreacheryCardType.Chemistry) || cardType == TreacheryCardType.ShieldAndAntidote;
            case TreacheryCardType.Poison: return cardType == TreacheryCardType.PoisonTooth || cardType == TreacheryCardType.ProjectileAndPoison;
            case TreacheryCardType.Shield: return cardType == TreacheryCardType.ShieldAndAntidote;
            case TreacheryCardType.ProjectileDefense: return cardType == TreacheryCardType.Shield || cardType == TreacheryCardType.ShieldAndAntidote;
            case TreacheryCardType.Projectile: return (asWeapon && cardType == TreacheryCardType.WeirdingWay) || cardType == TreacheryCardType.ProjectileAndPoison;
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
            var deal = new Deal
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
                if (!Player.ToldTraitors.Contains(hero)) Player.ToldTraitors.Add(hero);
            }
            else if (IsNo)
            {
                if (!Player.ToldNonTraitors.Contains(hero)) Player.ToldNonTraitors.Add(hero);
            }
        }
        else if (Game.LatestClairvoyance.Question == ClairvoyanceQuestion.LeaderAsFacedancer)
        {
            var hero = Game.LatestClairvoyance.Parameter1 as IHero;

            if (Answer == ClairVoyanceAnswer.Yes)
            {
                if (!Player.ToldFaceDancers.Contains(hero)) Player.ToldFaceDancers.Add(hero);
            }
            else if (Answer == ClairVoyanceAnswer.No)
            {
                if (!Player.ToldNonFaceDancers.Contains(hero)) Player.ToldNonFaceDancers.Add(hero);
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