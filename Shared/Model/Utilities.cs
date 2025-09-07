using System;
using System.Text.Json.Nodes;
using System.Globalization;
    
namespace Treachery.Shared;

public static class Utilities
{
    public static List<T> CloneList<T>(List<T> toClone) where T : ICloneable
        => toClone.Select(item => (T)item.Clone()).ToList();

    public static Dictionary<TX, TY> CloneObjectDictionary<TX, TY>(Dictionary<TX, TY> toClone) where TY : ICloneable
        => toClone.ToDictionary(item => item.Key, item => (TY)item.Value.Clone());

    public static Dictionary<TX, TY> CloneEnumDictionary<TX, TY>(Dictionary<TX, TY> toClone) where TY : struct, Enum
        => toClone.ToDictionary(item => item.Key, item => item.Value);

    private static readonly JsonSerializerOptions Options = new() { IncludeFields = true };

    private const string NewtonsoftIndicator = "Treachery.Shared.GameState";

    public static T Deserialize<T>(string serializedValue)
    {
        if (serializedValue.Length < 128 || !serializedValue[..128].Contains(NewtonsoftIndicator))
            return JsonSerializer.Deserialize<T>(serializedValue, Options);
        
        var convertedValue = ConvertNewtonsoftJsonToPlainJson(serializedValue);
        var res = JsonSerializer.Deserialize<T>(convertedValue, Options);
        return res;
    }

    private static string ConvertNewtonsoftJsonToPlainJson(string value)
    {
        var jsonNode = JsonNode.Parse(value);
        ReplaceTypeMetadataAndNestedArrays(jsonNode);
        return jsonNode?.ToJsonString(new JsonSerializerOptions {WriteIndented = false}) ?? string.Empty;
    }

    private static void ReplaceTypeMetadataAndNestedArrays(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            if (obj.ContainsKey("$type"))
            {
                var eventType = DetermineGameEvent(obj["$type"]!.GetValue<string>());
                if (eventType != null)
                {
                    obj["$type"] = eventType.Name;
                }
                else
                {
                    obj.Remove("$type");
                }
            }
            
            if (obj.Count == 1 && obj.ContainsKey("$values") && obj["$values"] is JsonArray valuesArray)
            {
                var newArray = new JsonArray();
                foreach (var item in valuesArray)
                {
                    var clone = item.DeepClone();
                    ReplaceTypeMetadataAndNestedArrays(clone);
                    newArray.Add(clone);
                }
                obj.ReplaceWith(newArray);
            }
            else
            {
                foreach (var key in ((IDictionary<string, JsonNode>)obj).Keys)
                {
                    ReplaceTypeMetadataAndNestedArrays(obj[key]);
                }                
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                ReplaceTypeMetadataAndNestedArrays(item);
            }
        }
    }

    private static Type DetermineGameEvent(string fullTypeName)
    {
        var foundType = Type.GetType(fullTypeName);
        return foundType is { IsAbstract: false, IsInterface: false } && foundType.IsSubclassOf(typeof(GameEvent)) ? foundType : null;
    }

    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, Options);
    
    public static string ScaleFont(string font, float scale)
    {
        var fontsizeLower = font.ToLower();
        var result = "";

        foreach (var fontDefinitionPart in fontsizeLower.Split(' ')) {

            if (fontDefinitionPart.Contains("px")) {

                if (float.TryParse(fontDefinitionPart.Remove(fontDefinitionPart.IndexOf("px", StringComparison.InvariantCulture)), out var fontsizeNumber))
                {
                    result += " " + Px(scale * fontsizeNumber);
                }
                else
                {
                    result += " " + fontDefinitionPart;
                }
            }
            else
            {
                result += " " + fontDefinitionPart;
            }
        }

        return result;
    }
    
    public static string Px(double x)
        => x is > -0.001 and < 0.001 ? "0" : x.ToString(CultureInfo.InvariantCulture) + "px";

    public static string Round(double x)
        => x.ToString(CultureInfo.InvariantCulture);
}