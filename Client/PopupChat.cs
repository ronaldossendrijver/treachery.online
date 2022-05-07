/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Client
{
#pragma warning disable IDE1006 // Naming Styles

    public abstract class PopupChatCommand
    {
        public abstract string type { get; set; }
    }

    public class PopupChatInitialization : PopupChatCommand
    {
        public override string type { get; set; } = "PopupChatInitialize";
        public string[] playerNames { get; set; }
        public string[] playerStyles { get; set; }

        public PopupChatMessage[] messages { get; set; }
    }

    public class PopupChatMessage : PopupChatCommand
    {
        public override string type { get; set; } = "PopupChatMessage";
        public string style { get; set; }
        public string body { get; set; }
    }

    public class PopupChatClear : PopupChatCommand
    {
        public override string type { get; set; } = "PopupChatClear";
    }

#pragma warning restore IDE1006 // Naming Styles
}
