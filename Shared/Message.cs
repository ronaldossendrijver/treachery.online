/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class Message
    {
        public static IDescriber DefaultDescriber = null;

        private static int Counter = 0;

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

        public Expression Expression { get; private set; }

        public override string ToString()
        {
            if (DefaultDescriber != null)
            {
                return ToString(DefaultDescriber) + "*";
            }
            else
            {
                return string.Join("", Expression.Elements);
            }
        }

        public string ToString(IDescriber describer) => string.Join("", Expression.Elements.Select(e => describer.Describe(e)));

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

        public Expression Expression { get; private set; }

        public static MessagePart Express(params object[] list)
        {
            return new MessagePart(new Expression(list));
        }

        public static MessagePart ExpressIf(bool condition, params object[] list)
        {
            if (condition)
            {
                return new MessagePart(new Expression(list));
            }
            else
            {
                return new MessagePart(new Expression());
            }
        }

        public string ToString(IDescriber describer) => string.Join("", Expression.Elements.Select(e => describer.Describe(e)));

        public override string ToString()
        {
            if (Message.DefaultDescriber != null)
            {
                return ToString(Message.DefaultDescriber) + "*";
            }
            else
            {
                return string.Join("", Expression.Elements);
            }
        }
    }
}
