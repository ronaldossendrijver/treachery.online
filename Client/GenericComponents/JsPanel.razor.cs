namespace Treachery.Client.GenericComponents
{
    public partial class JsPanel
    {
    }

    public class JsPanelTheme
    {
        public static readonly JsPanelTheme TRANSPARANT = new JsPanelTheme() { bgContent = "rgba(0,0,0,0)" };
        public static readonly JsPanelTheme DEFAULT = new JsPanelTheme() { };

        public string bgPanel { get; set; } = "";
        public string bgContent { get; set; } = "";
        public string colorHeader { get; set; } = "";
        public string colorContent { get; set; } = "";
        public string border { get; set; } = "";
    }
}
