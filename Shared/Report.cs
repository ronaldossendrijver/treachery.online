/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public class Report
    {
        public MainPhase About { private set; get; }

        public Report(MainPhase about)
        {
            About = about;
        }

        public LinkedList<Message> Messages = new LinkedList<Message>();

        public void Express(params object[] list)
        {
            Add(Message.Express(list));
        }

        public void ExpressTo(Faction to, params object[] list)
        {
            Add(Message.ExpressTo(Faction.None, to, list));
        }

        public void Add(string m, params object[] list)
        {
            Add(new Message(m, list));
        }
        public void Add(Faction f, string m, params object[] list)
        {
            Add(new Message(f, m, list));
        }

        public void Add(Faction from, Faction to, string m, params object[] list)
        {
            Add(new Message(from, to, m, list));
        }

        public void Add(Message m)
        {
            Messages.AddLast(m);
        }

        public void Add(GameEvent e)
        {
            Messages.AddLast(e.GetMessage());
        }

        public string Title => Skin.Current.Format("{0} Report", About);
    }

}
