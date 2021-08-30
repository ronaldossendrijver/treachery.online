using Treachery.Shared;

namespace Treachery.Client
{
    public class LocationEventArgs
    {
        public Location Location { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool AltKey { get; set; }
    }

    public class TerritoryEventArgs
    {
        public Territory Territory { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool AltKey { get; set; }
    }
}
