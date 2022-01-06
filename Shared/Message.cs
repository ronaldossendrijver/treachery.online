/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Message
    {
        private static int Counter = 0;

        public int Nr { get; } = Counter++;

        public Faction Initiator { get; set; }

        public Faction Target { get; set; }

        private string _unformattedBody;

        private object[] _parameters;

        private Expression _expression;

        private string _toString = null;

        public Message(Expression e)
        {
            _expression = e;
            Initiator = Faction.None;
        }

        public Message(Faction f, Expression e)
        {
            _expression = e;
            Initiator = f;
        }

        public Message(Faction from, Faction to, Expression e)
        {
            _expression = e;
            Initiator = from;
            Target = to;
        }

        public Message(string m, params object[] list)
        {
            _unformattedBody = m;
            _parameters = list;
            Initiator = Faction.None;
        }

        public Message(Faction f, string m, params object[] list)
        {
            _unformattedBody = m;
            _parameters = list;
            Initiator = f;
        }

        public Message(Faction from, Faction to, string m, params object[] list)
        {
            _unformattedBody = m;
            _parameters = list;
            Initiator = from;
            Target = to;
        }

        public Expression Expression
        {
            get
            {
                if (_expression == null)
                {
                    _expression = DetermineExpression(_unformattedBody, _parameters);
                }

                return _expression;
            }
        }

        public static Expression DetermineExpression(string unformattedBody, object[] parameters)
        {
            var elements = new List<object>();

            foreach (var part in unformattedBody.Split('{'))
            {
                int index = part.IndexOf('}');

                if (index > 0)
                {
                    int parameternr = int.Parse(part.Substring(0, index));
                    elements.Add(parameters[parameternr]);

                    if (index < part.Length - 1)
                    {
                        elements.Add(part.Substring(index + 1));
                    }
                }
                else
                {
                    if (part.Length > 0) {
                        
                        elements.Add(part);
                    }
                }
            }

            return new Expression(elements);
        }

        public override string ToString()
        {
            if (_toString == null)
            {
                if (_expression == null)
                {
                    _toString = Skin.Current.Format(_unformattedBody, _parameters);
                }
                else
                {
                    _toString = string.Join("", Expression.Elements.Select(e => Skin.Current.Describe(e)));
                }
            }

            return _toString;
        }

        public static Message Express(params object[] list)
        {
            return new Message(new Expression(list));
        }

        public static Message ExpressTo(Faction from, Faction to, params object[] list)
        {
            return new Message(from, to, new Expression(list));
        }
    }

    public class MessagePart
    {
        private string _unformattedBody;

        private object[] _parameters;

        private Expression _expression;

        public MessagePart()
        {
            _expression = new Expression();
        }

        public MessagePart(Expression e)
        {
            _expression = e;
        }

        public MessagePart(string unformattedBody, params object[] parameters)
        {
            _unformattedBody = unformattedBody;
            _parameters = parameters;
        }

        public Expression Expression
        {
            get
            {
                if (_expression == null)
                {
                    _expression = Message.DetermineExpression(_unformattedBody, _parameters);
                }

                return _expression;
            }
        }

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

        private string _toString = null;
        public override string ToString()
        {
            if (_toString == null)
            {
                if (_expression == null)
                {
                    _toString = Skin.Current.Format(_unformattedBody, _parameters);
                }
                else
                {
                    _toString = string.Join("", Expression.Elements.Select(e => Skin.Current.Describe(e)));
                }
            }

            return _toString;
        }
    }
}
