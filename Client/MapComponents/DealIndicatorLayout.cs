using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Treachery.Client.MapComponents;

public sealed record DealIndicator(Deal Deal, double X, double Y, double Size);

/// <summary>
/// Places deal indicators in small rings around the logo of the faction that offered the deal, in the nearest spot that keeps
/// clear of player names, logos, ally logos, bid texts, items in front of shields, territories and other indicators.
/// Spots on the black outer ring of the map are preferred over spots outside it.
/// </summary>
public sealed partial record DealIndicatorLayout(IReadOnlyList<DealIndicator> Indicators, double VisualRadius)
{
    private const double PreviousSizeFactor = 0.8;
    private const double LogoScale = 1.5;
    // The frame of the original indicator (1px padding + 1px border per side); the logo now also fills the former padding.
    public const double FrameWidth = 4;
    public const double GapFactor = 0.12;

    public static double IndicatorSize(double tokenRadius) =>
        LogoScale * (PreviousSizeFactor * tokenRadius - FrameWidth) + FrameWidth;
    private const int MaximumRings = 40;
    private const int FineRings = 3;
    private const double CharacterWidthFactor = 0.42;
    private const double LineHeightFactor = 1.2;

    public static bool CanPlaceOnBoard(Game game, Deal deal) =>
        game.CurrentPhase > Phase.SelectingFactions
        && game.Players.Any(player => player.Faction == deal.BoundFaction && player.Seat >= 0);

    public static DealIndicatorLayout Create(Game game, Skin skin)
    {
        var tokenRadius = (double)skin.PlayerTokenRadius;
        var size = IndicatorSize(tokenRadius);
        var radius = size / 2;
        var gap = GapFactor * tokenRadius;
        var seated = game.Players.Where(player => player.Seat >= 0).ToArray();
        var obstacles = Obstacles(game, skin, seated);
        var indicators = new List<DealIndicator>();
        var visualRadius = 0d;
        var minimumSectorSpacing = Math.Floor((double)Map.NumberOfSectors / game.MaximumPlayers);
        var minimumSeatAngle = minimumSectorSpacing * 2 * Math.PI / Map.NumberOfSectors;
        var innerLimit = skin.PlanetRadius + 0.15 * tokenRadius;
        // The black outer ring of the map ends at the map radius; indicators preferably stay on it.
        var outerLimit = (double)skin.MapRadius;
        var currentDeals = DealAccepted.CurrentDeals(game).Where(deal => CanPlaceOnBoard(game, deal)).ToArray();

        foreach (var player in seated)
        {
            var deals = currentDeals.Where(deal => deal.BoundFaction == player.Faction).ToArray();
            if (deals.Length == 0) continue;

            var seatAngle = PlayerTokenLayout.SeatAngle(game, player.Seat);
            var logo = PlayerTokenLayout.Position(game, skin, player.Seat);
            var candidates = Candidates(logo, tokenRadius, radius, gap)
                .Where(point => WithinSeat(skin, point, radius, seatAngle, minimumSeatAngle) && DistanceToCenter(skin, point) - radius >= innerLimit)
                .ToArray();
            var onRing = candidates.Where(point => DistanceToCenter(skin, point) + radius <= outerLimit + Tolerance).ToArray();

            foreach (var deal in deals)
            {
                bool FreeOfIndicators(PointD point) => indicators.All(other =>
                    Distance(point, Center(other)) >= 2 * radius + gap - 0.001);

                bool Free(PointD point) => FreeOfIndicators(point) && obstacles.All(obstacle => !obstacle.Hits(point, radius + gap));

                var spot = onRing.Where(Free).Cast<PointD?>().FirstOrDefault()
                    ?? candidates.Where(Free).Cast<PointD?>().FirstOrDefault()
                    ?? candidates.Where(FreeOfIndicators).Cast<PointD?>().FirstOrDefault()
                    ?? logo;

                indicators.Add(new DealIndicator(deal, spot.X - radius, spot.Y - radius, size));
                visualRadius = Math.Max(visualRadius, DistanceToCenter(skin, spot) + radius);
            }
        }

        return new DealIndicatorLayout(indicators, visualRadius);
    }

    /// <summary>
    /// Candidate centers on concentric rings around a logo, nearest ring first and, within a ring, nearest to "below the logo"
    /// first. Spots above the logo (beyond the player name) are only used when nothing closer around the logo is free.
    /// </summary>
    private static IEnumerable<PointD> Candidates(PointD logo, double tokenRadius, double radius, double gap)
    {
        var pitch = 2 * radius + gap;
        var candidates = new List<(PointD Point, double Cost)>();
        var first = 1.5 * tokenRadius + gap + radius;
        var last = first + (MaximumRings - 1) * pitch;
        for (var distance = first; distance <= last + 0.001;)
        {
            // Sample the area right around the logo finely, so the nearest free spot between texts and tokens is found.
            var fine = distance < first + FineRings * pitch;
            var spacing = fine ? pitch / 2 : pitch;
            var count = Math.Max(6, (int)Math.Floor(2 * Math.PI * distance / spacing));
            var step = 2 * Math.PI / count;
            for (var index = 0; index < count; index++)
            {
                var offset = (index + 1) / 2 * (index % 2 == 0 ? -1 : 1);
                var angle = Math.PI / 2 + offset * step;
                var point = new PointD(logo.X + Math.Cos(angle) * distance, logo.Y + Math.Sin(angle) * distance);
                var abovePenalty = point.Y < logo.Y - 1.5 * tokenRadius ? 0.5 * tokenRadius : 0;
                candidates.Add((point, distance + abovePenalty + 0.001 * Math.Abs(offset)));
            }

            distance += fine ? pitch / 4 : pitch;
        }

        return candidates.OrderBy(candidate => candidate.Cost).Select(candidate => candidate.Point);
    }

    private static List<Obstacle> Obstacles(Game game, Skin skin, IEnumerable<Player> players)
    {
        var tokenRadius = (double)skin.PlayerTokenRadius;
        var nameFontSize = FontSize(skin.PLAYERNAME_FONT, 10);
        var result = new List<Obstacle>();

        foreach (var player in players)
        {
            var logo = PlayerTokenLayout.Position(game, skin, player.Seat);

            // Highlighted logos are 1.5 times larger; reserve that space so indicators don't move when players start or stop acting.
            result.Add(new CircleObstacle(logo, 1.5 * tokenRadius));

            if (player.HasAlly)
                result.Add(new CircleObstacle(new PointD(logo.X + 0.6 * tokenRadius, logo.Y + 0.85 * tokenRadius), 0.7 * tokenRadius));

            foreach (var item in PlayerTokenLayout.PeripheralItems(game, skin, player))
            {
                result.Add(new CircleObstacle(item.Center, item.Radius));
                if (!string.IsNullOrEmpty(item.Text))
                    result.Add(TextObstacle(item.Center, item.Text, FontSize(item.Font ?? string.Empty, 8), item.MaxTextWidth));
            }

            // Player names are drawn centered above the top of the (normal or highlighted) logo, shifted up by 40% of their height.
            var nameWidth = TextWidth(player.Name, nameFontSize);
            var maxNameWidth = 6 * tokenRadius;
            var nameHeight = Math.Max(1, Math.Ceiling(nameWidth / maxNameWidth - 0.05)) * LineHeightFactor * nameFontSize;
            nameWidth = Math.Min(nameWidth, maxNameWidth);
            result.Add(new RectangleObstacle(
                logo.X - nameWidth / 2, logo.Y - 1.5 * tokenRadius - 0.4 * nameHeight,
                logo.X + nameWidth / 2, logo.Y - tokenRadius + 0.6 * nameHeight));

            // Bid texts are not obstacles: they are temporary and ignore the mouse, so indicators below them stay hoverable.
        }

        return result;
    }

    /// <summary>
    /// The area of a MapText centered at a point: it wraps at its maximum width and is shifted up by 40% of its height.
    /// </summary>
    private static RectangleObstacle TextObstacle(PointD center, string text, double fontSize, double? maxWidth)
    {
        var width = TextWidth(text, fontSize);
        var lines = maxWidth is > 0 ? Math.Max(1, Math.Ceiling(width / maxWidth.Value - 0.05)) : 1;
        if (maxWidth is > 0) width = Math.Min(width, maxWidth.Value);
        var height = lines * LineHeightFactor * fontSize;
        return new RectangleObstacle(center.X - width / 2, center.Y - 0.4 * height, center.X + width / 2, center.Y + 0.6 * height);
    }

    private static double TextWidth(string? text, double fontSize) =>
        (string.IsNullOrEmpty(text) ? 0 : text.Length) * CharacterWidthFactor * fontSize + 4;

    private static double FontSize(string font, double fallback)
    {
        var match = FontSizeRegex().Match(font ?? string.Empty);
        return match.Success ? double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : fallback;
    }

    private static bool WithinSeat(Skin skin, PointD point, double radius, double seatAngle, double minimumSeatAngle)
    {
        var distance = DistanceToCenter(skin, point);
        var angle = Math.Atan2(point.Y - skin.PlanetCenter.Y, point.X - skin.PlanetCenter.X);
        var offset = Math.Atan2(Math.Sin(angle - seatAngle), Math.Cos(angle - seatAngle));
        return Math.Abs(offset) + Math.Asin(Math.Min(1, radius / distance)) < minimumSeatAngle / 2;
    }

    private static PointD Center(DealIndicator indicator) =>
        new(indicator.X + indicator.Size / 2, indicator.Y + indicator.Size / 2);

    private static double DistanceToCenter(Skin skin, PointD point) => Distance(point, skin.PlanetCenter);

    private static double Distance(PointD a, PointD b) => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

    [GeneratedRegex(@"(\d+(?:\.\d+)?)px")]
    private static partial Regex FontSizeRegex();

    private abstract record Obstacle
    {
        public abstract bool Hits(PointD center, double clearance);
    }

    private sealed record CircleObstacle(PointD Center, double Radius) : Obstacle
    {
        public override bool Hits(PointD center, double clearance) => Distance(center, Center) < Radius + clearance - Tolerance;
    }

    private sealed record RectangleObstacle(double Left, double Top, double Right, double Bottom) : Obstacle
    {
        public override bool Hits(PointD center, double clearance)
        {
            var dx = center.X - Math.Clamp(center.X, Left, Right);
            var dy = center.Y - Math.Clamp(center.Y, Top, Bottom);
            return dx * dx + dy * dy < Math.Pow(clearance - Tolerance, 2);
        }
    }

    // Candidates exactly touching their required clearance are allowed despite floating-point rounding.
    private const double Tolerance = 0.001;
}
