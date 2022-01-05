/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public class Message
    {
        private static int Counter = 0;

        public string UnformattedBody { private get; set; }

        public object[] Parameters { get; set; }

        public int Nr { get; } = Counter++;

        public Faction Initiator { get; set; }

        public Faction Target { get; set; }

        private Expression _expression;

        public override string ToString()
        {
            return Skin.Current.Format(UnformattedBody, Parameters);
        }

        public Message(string m, params object[] list)
        {
            UnformattedBody = m;
            Parameters = list;
            Initiator = Faction.None;
        }

        public Message(Faction f, string m, params object[] list)
        {
            UnformattedBody = m;
            Parameters = list;
            Initiator = f;
        }

        public Message(Faction from, Faction to, string m, params object[] list)
        {
            UnformattedBody = m;
            Parameters = list;
            Initiator = from;
            Target = to;
        }

        public Expression Expression
        {
            get
            {
                if (_expression == null)
                {
                    _expression = DetermineExpression(UnformattedBody, Parameters);
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
                    elements.Add(part.Substring(index + 1));
                    
                }
                else
                {
                    elements.Add(part);
                }
            }

            return new Expression(elements);
        }
    }

    public class MessagePart
    {
        public string UnformattedBody { private get; set; }

        public object[] Parameters { get; set; }

        private Expression _expression;

        public MessagePart(string unformattedBody, params object[] parameters)
        {
            UnformattedBody = unformattedBody;
            Parameters = parameters;
        }

        public Expression Expression
        {
            get
            {
                if (_expression == null)
                {
                    _expression = Message.DetermineExpression(UnformattedBody, Parameters);
                }

                return _expression;
            }
        }

        public override string ToString()
        {
            return Skin.Current.Format(UnformattedBody, Parameters);
        }
    }
}
