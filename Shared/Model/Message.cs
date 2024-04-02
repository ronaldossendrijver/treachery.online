/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class Message
{
    public static IDescriber DefaultDescriber = null;

    private static int Counter;

    public int Nr { get; } = Counter++;

    public Faction Target { get; set; }

    private Message(Expression e)
    {
        Expression = e;
    }

    private Message(Faction to, Expression e)
    {
        Expression = e;
        Target = to;
    }

    public Expression Expression { get; }

    public override string ToString()
    {
        if (DefaultDescriber != null)
            return ToString(DefaultDescriber) + "*";
        return string.Join("", Expression.Elements);
    }

    public string ToString(IDescriber describer)
    {
        return string.Join("", Expression.Elements.Select(e => describer.Describe(e)));
    }

    public static Message Express(params object[] list)
    {
        return new Message(new Expression(list));
    }

    public static Message ExpressTo(Faction to, params object[] list)
    {
        return new Message(to, new Expression(list));
    }
}

public class MessagePart
{
    private MessagePart()
    {
        Expression = new Expression();
    }

    private MessagePart(Expression e)
    {
        Expression = e;
    }

    public Expression Expression { get; }

    public static MessagePart Express(params object[] list)
    {
        return new MessagePart(new Expression(list));
    }

    public static MessagePart ExpressIf(bool condition, params object[] list)
    {
        if (condition)
            return new MessagePart(new Expression(list));
        return new MessagePart(new Expression());
    }

    public string ToString(IDescriber describer)
    {
        return string.Join("", Expression.Elements.Select(e => describer.Describe(e)));
    }

    public override string ToString()
    {
        if (Message.DefaultDescriber != null)
            return ToString(Message.DefaultDescriber) + "*";
        return string.Join("", Expression.Elements);
    }
}