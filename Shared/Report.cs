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

        public void ExpressIf(bool condition, params object[] list)
        {
            if (condition)
            {
                Add(Message.Express(list));
            }
        }

        public void Express(params object[] list)
        {
            Add(Message.Express(list));
        }

        public void ExpressTo(Faction to, params object[] list)
        {
            Add(Message.ExpressTo(to, list));
        }

        public void Express(Message m)
        {
            Add(m);
        }

        public void Express(GameEvent e)
        {
            Messages.AddLast(e.GetMessage());
        }

        private void Add(Message m)
        {
            Messages.AddLast(m);
        }

        public string Title => Skin.Current.Format("{0} Report", About);
    }

}
