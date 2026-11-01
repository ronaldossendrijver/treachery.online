using System.Linq;

namespace Treachery.Client.MapComponents;

public sealed record PeripheralItem(PointD Center, double Radius, string? Text = null, string? Font = null, double? MaxTextWidth = null);

public static class PlayerTokenLayout
{
    public static double SeatAngle(Game game, int seat) =>
        (4.5 - Math.Floor((double)seat * Map.NumberOfSectors / game.MaximumPlayers))
        * 2 * Math.PI / Map.NumberOfSectors;

    public static PointD Position(Game game, Skin skin, int seat) =>
        OnCircle(skin, SeatAngle(game, seat), skin.MapRadius);

    public static IEnumerable<PointD> PeripheralCenters(Game game, Skin skin, Player player) =>
        PeripheralItems(game, skin, player).Select(item => item.Center);

    /// <summary>
    /// The items drawn in front of a player's shield (see Players.razor), with the radius of their image and any text drawn on top of it.
    /// </summary>
    public static IEnumerable<PeripheralItem> PeripheralItems(Game game, Skin skin, Player player)
    {
        var angle = SeatAngle(game, player.Seat) + 2 * Math.PI / 180;
        var radius = 1.06 * skin.MapRadius;
        var step = 2 * Math.PI / 90;
        var tokenRadius = (double)skin.PlayerTokenRadius;
        var forceRadius = (double)skin.FORCETOKEN_RADIUS;

        foreach (var homeworld in player.HomeWorlds)
        {
            angle += 0.06;
            yield return new PeripheralItem(OnCircle(skin, angle, radius), tokenRadius);
            angle += 0.02;
        }

        foreach (var token in player.TechTokens)
        {
            angle += step;
            yield return new PeripheralItem(OnCircle(skin, angle, radius), tokenRadius);
        }

        if (player.Faction == Faction.Pink && game.PinkLoyalLeader != null)
        {
            angle += step;
            yield return new PeripheralItem(OnCircle(skin, angle, radius), tokenRadius);
        }

        foreach (var leader in game.GetSkilledLeaders(player).Where(game.IsInFrontOfShield))
        {
            angle += step;
            yield return new PeripheralItem(OnCircle(skin, angle, radius), tokenRadius,
                skin.Describe(game.Skill(leader)), skin.SKILL_FONT, 3 * tokenRadius);
        }

        if (player.Faction == Faction.Pink && player.Ambassadors.Any())
        {
            angle += step;
            // Ambassadors are drawn as a staggered stack that extends half a token to the right of the item center.
            yield return new PeripheralItem(OnCircle(skin, angle, radius), 1.2 * tokenRadius);
        }

        if (player.Faction == Faction.White && game.LatestRevealedNoFieldValue >= 0)
        {
            angle += step;
            yield return new PeripheralItem(OnCircle(skin, angle, radius), forceRadius,
                game.LatestRevealedNoFieldValue.ToString(), skin.FORCETOKEN_FONT);
        }

        var totalSpiceToReceiveLater = player.Bribes + player.BankedResources + player.Extortion;
        if (totalSpiceToReceiveLater > 0)
        {
            angle += step;
            yield return new PeripheralItem(OnCircle(skin, angle, radius), forceRadius,
                totalSpiceToReceiveLater.ToString(), skin.RESOURCETOKEN_FONT);
        }

        if (player.ResourcesAfterBidding > 0)
        {
            angle += step;
            yield return new PeripheralItem(OnCircle(skin, angle, radius), forceRadius,
                skin.Format("Subbed {0}", Concept.Resource), skin.RESOURCETOKEN_FONT);
        }
    }

    private static PointD OnCircle(Skin skin, double angle, double radius) =>
        new(skin.PlanetCenter.X + Math.Cos(angle) * radius, skin.PlanetCenter.Y + Math.Sin(angle) * radius);
}
