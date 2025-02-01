/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Globalization;

namespace Treachery.Shared;

public class ClairVoyancePlayed : GameEvent
{
    #region Construction

    public ClairVoyancePlayed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public ClairVoyancePlayed()
    {
    }

    #endregion Construction

    #region Properties

    public ClairvoyanceQuestion Question { get; set; }

    public Faction Target { get; set; }

    public bool IsAbout(TreacheryCardType type)
    {
        return Parameter1 is TreacheryCardType t && t == type;
    }

    public string QuestionParameter1 { get; set; }

    [JsonIgnore]
    public object Parameter1
    {
        get
        {
            return Question switch
            {
                ClairvoyanceQuestion.CardTypeInBattle or
                    ClairvoyanceQuestion.CardTypeAsDefenseInBattle or
                    ClairvoyanceQuestion.CardTypeAsWeaponInBattle or
                    ClairvoyanceQuestion.HasCardTypeInHand => Enum.Parse<TreacheryCardType>(QuestionParameter1),

                ClairvoyanceQuestion.LeaderAsFacedancer or
                    ClairvoyanceQuestion.LeaderAsTraitor or
                    ClairvoyanceQuestion.LeaderInBattle => LeaderManager.HeroLookup.Find(int.Parse(QuestionParameter1)),

                ClairvoyanceQuestion.Prediction => Enum.Parse<Faction>(QuestionParameter1),

                ClairvoyanceQuestion.WillAttackX => Game.Map.TerritoryLookup.Find(int.Parse(QuestionParameter1)),

                ClairvoyanceQuestion.DialOfMoreThanXInBattle => float.Parse(QuestionParameter1, CultureInfo.InvariantCulture),

                _ => null
            };
        }

        set
        {
            if (value == null)
                QuestionParameter1 = null;
            else
                QuestionParameter1 = Question switch
                {
                    ClairvoyanceQuestion.DialOfMoreThanXInBattle => ((float)value).ToString(CultureInfo.InvariantCulture),

                    _ => value.ToString()
                };
        }
    }

    public string QuestionParameter2 { get; set; }

    [JsonIgnore]
    public object Parameter2
    {
        get
        {
            return Question switch
            {
                ClairvoyanceQuestion.Prediction => int.Parse(QuestionParameter2),
                _ => null
            };
        }

        set
        {
            if (value == null)
                QuestionParameter2 = null;
            else
                QuestionParameter2 = value.ToString();
        }
    }

    public Message GetQuestion()
    {
        if (Question == ClairvoyanceQuestion.None)
            return Message.Express("");
        return Express(Question, Parameter1, Parameter2);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static IEnumerable<Faction> ValidTargets(Game g, Player p)
    {
        return g.PlayersOtherThan(p);
    }

    public static IEnumerable<ClairvoyanceQuestion> ValidQuestions(Game g, Faction target)
    {
        var allValues = Enumerations.GetValues<ClairvoyanceQuestion>();
        var targetPlayer = g.GetPlayer(target);
        if (targetPlayer == null || !g.IsBot(targetPlayer))
            return allValues;
        return allValues.Where(v => AppliesToBot(v, g, targetPlayer));
    }

    private static bool AppliesToBot(ClairvoyanceQuestion q, Game g, Player p)
    {
        return q switch
        {
            ClairvoyanceQuestion.CardTypeInBattle or
                ClairvoyanceQuestion.CardTypeAsDefenseInBattle or
                ClairvoyanceQuestion.CardTypeAsWeaponInBattle or
                ClairvoyanceQuestion.LeaderInBattle or
                ClairvoyanceQuestion.DialOfMoreThanXInBattle => g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p),

            _ => true
        };
    }

    public static Message Express(ClairvoyanceQuestion q, object parameter1 = null, object parameter2 = null)
    {
        var p1 = parameter1 ?? "...";
        var p2 = parameter2 ?? "...";

        return q switch
        {
            ClairvoyanceQuestion.None => Message.Express("Any question"),
            ClairvoyanceQuestion.CardTypeInBattle => Message.Express("In this battle, is one of your cards a ", p1),
            ClairvoyanceQuestion.CardTypeAsDefenseInBattle => Message.Express("In this battle, will you use ", p1, " as defense?"),
            ClairvoyanceQuestion.CardTypeAsWeaponInBattle => Message.Express("In this battle, will you use ", p1, " as weapon?"),
            ClairvoyanceQuestion.HasCardTypeInHand => Message.Express("Do you own a ", p1, "?"),
            ClairvoyanceQuestion.LeaderInBattle => Message.Express("In this battle, is your leader ", p1, "?"),
            ClairvoyanceQuestion.DialOfMoreThanXInBattle => Message.Express("In this battle, is your dial higher than ", p1, "?"),
            ClairvoyanceQuestion.LeaderAsFacedancer => Message.Express("Is ", p1, " one of your face dancers?"),
            ClairvoyanceQuestion.LeaderAsTraitor => Message.Express("Is ", p1, " one of your traitors?"),
            ClairvoyanceQuestion.Prediction => Message.Express("Did you predict a ", p1, " win in turn ", p2, "?"),
            ClairvoyanceQuestion.WillAttackX => Message.Express("Will you ship or move to ", p1, " this turn?"),
            _ => Message.Express("unknown question")
        };
    }

    public static bool IsInScopeOf(bool asWeapon, TreacheryCardType cardType, TreacheryCardType inScopeOfQuestion)
    {
        if (cardType == inScopeOfQuestion)
            return true;
        switch (inScopeOfQuestion)
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
        var card = Card;

        if (card != null)
        {
            Game.Discard(card);
            Log();
            Game.Stone(Milestone.Clairvoyance);
        }

        if (Target != Faction.None)
        {
            Game.LatestClairvoyance = this;
            Game.LatestClairvoyanceQandA = null;
            Game.LatestClairvoyanceBattle = Game.CurrentBattle;
            Game.PhasePausedByClairvoyance = Game.CurrentPhase;
            Game.Enter(Phase.Clairvoyance);
        }
    }

    private TreacheryCard Card
    {
        get
        {
            TreacheryCard result = null;

            if (Player.Occupies(Game.Map.Shrine))
                result = Karma.ValidKarmaCards(Game, Player).FirstOrDefault(c => c.Type == TreacheryCardType.Useless);
            
            if (result == null)
                result = Player.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Clairvoyance);
            
            if (result == null && Player.Occupies(Game.Map.Shrine))
                result = Karma.ValidKarmaCards(Game, Player).FirstOrDefault();

            return result;
        }
    }

    public override Message GetMessage()
    {
        if (Target == Faction.None) return Message.Express(Initiator, " perform ", TreacheryCardType.Clairvoyance);

        if (Question == ClairvoyanceQuestion.None)
            return Message.Express("By ", TreacheryCardType.Clairvoyance, ", ", Initiator, " ask ", Target, " a question");
        return Message.Express("By ", TreacheryCardType.Clairvoyance, ", ", Initiator, " ask ", Target, ": \"", GetQuestion(), "\"");
    }

    public override Message GetShortMessage()
    {
        return Message.Express(Initiator, " perform ", TreacheryCardType.Clairvoyance);
    }

    #endregion Execution
}