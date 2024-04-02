/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class Report
{
    public MainPhase About { private set; get; }

    public Report(MainPhase about)
    {
        About = about;
    }

    public LinkedList<Message> Messages = new();

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
}