using System.Text.Json.Nodes;
using Treachery.Shared;

namespace Treachery.Client.Test;

internal static class HistoricalGameEventComparer
{
    private static readonly IReadOnlyDictionary<Type, string[]> ExecutionAssignedMembers =
        new Dictionary<Type, string[]>
        {
            [typeof(AutomationConfigured)] = [nameof(AutomationConfigured.AutomationRuleId)]
        };

    public static void AssertEquivalent(GameEvent expected, GameEvent actual, string context)
    {
        if (expected is Shipment expectedShipment && actual is Shipment actualShipment)
        {
            Assert.AreEqual(expectedShipment.NoFieldValue, actualShipment.NoFieldValue, context);
            Assert.AreEqual(expectedShipment.CunningNoFieldValue, actualShipment.CunningNoFieldValue, context);
        }

        var expectedNode = SerializeDecision(expected);
        var actualNode = SerializeDecision(actual);

        Assert.AreEqual(
            expectedNode.ToJsonString(),
            actualNode.ToJsonString(),
            $"{context}{Environment.NewLine}Expected: {expectedNode}{Environment.NewLine}Actual: {actualNode}");
    }

    public static bool AreEquivalent(GameEvent expected, GameEvent actual) =>
        SerializeDecision(expected).ToJsonString() == SerializeDecision(actual).ToJsonString();

    private static JsonNode SerializeDecision(GameEvent gameEvent)
    {
        var node = JsonNode.Parse(Utilities.Serialize<GameEvent>(gameEvent))
            ?? throw new InvalidOperationException("Could not serialize game event.");
        var json = node.AsObject();
        json.Remove(nameof(GameEvent.Time));
        foreach (var obsoleteMember in gameEvent.GetType().GetMembers()
                     .Where(member => member.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0))
        {
            json.Remove(obsoleteMember.Name);
        }

        foreach (var member in ExecutionAssignedMembers.GetValueOrDefault(gameEvent.GetType(), []))
        {
            json.Remove(member);
        }

        if (gameEvent is Shipment)
        {
            json.Remove("_noFieldValue");
            json.Remove("_cunningNoFieldValue");
            if (((Shipment)gameEvent).ShipmentType is not ShipmentType.ShipmentSiteToSite)
            {
                json.Remove("_fromId");
            }
        }

        if (gameEvent is LoserConcluded &&
            string.IsNullOrEmpty(json["_forcedKeptOrDiscardedCardIds"]?.GetValue<string>()))
        {
            json.Remove("_forcedKeptOrDiscardedCardIds");
        }

        if (gameEvent is AmbassadorActivated &&
            (json["_brownCardIds"] is null ||
             string.IsNullOrEmpty(json["_brownCardIds"]?.GetValue<string>())))
        {
            json.Remove("_brownCardIds");
        }

        if (gameEvent is ClairVoyancePlayed clairvoyance)
        {
            if (clairvoyance.Question == ClairvoyanceQuestion.None)
            {
                json.Remove(nameof(ClairVoyancePlayed.QuestionParameter1));
            }

            if (clairvoyance.Question != ClairvoyanceQuestion.Prediction)
            {
                json.Remove(nameof(ClairVoyancePlayed.QuestionParameter2));
            }
        }

        if (gameEvent is Battle battle)
        {
            json["TotalForces"] = battle.Forces + battle.ForcesAtHalfStrength;
            json["TotalSpecialForces"] =
                battle.SpecialForces + battle.SpecialForcesAtHalfStrength;
            json.Remove(nameof(Battle.Forces));
            json.Remove(nameof(Battle.ForcesAtHalfStrength));
            json.Remove(nameof(Battle.SpecialForces));
            json.Remove(nameof(Battle.SpecialForcesAtHalfStrength));
        }

        if (gameEvent is BlueAccompanies &&
            gameEvent.Game.Version < Game.LatestVersion)
        {
            json.Remove("_targetId");
        }

        if (gameEvent is PlacementEvent placement)
        {
            json["_forceLocations"] = PlacementEvent.ForceLocationsString(
                gameEvent.Game,
                placement.ForceLocations
                    .Where(item => item.Value.TotalAmountOfForces > 0)
                    .OrderBy(item => item.Key.Id)
                    .ToDictionary());
        }

        return node;
    }
}
