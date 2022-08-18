/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Web;
using Treachery.Shared;

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

        public static PopupChatMessage Construct(Game game, string myName, ChatMessage m)
        {
            var sourcePlayer = game.GetPlayer(m.SourcePlayerName);
            var sourceFaction = sourcePlayer != null ? sourcePlayer.Faction : Faction.None;

            return new PopupChatMessage()
            {
                body = HttpUtility.HtmlEncode(m.GetBodyIncludingPlayerInfo(myName, game, true).ToString(Skin.Current)),
                style = Skin.Current.GetFactionColor(sourceFaction)
            };
        }
    }

    public class PopupChatClear : PopupChatCommand
    {
        public override string type { get; set; } = "PopupChatClear";
    }

#pragma warning restore IDE1006 // Naming Styles
}
