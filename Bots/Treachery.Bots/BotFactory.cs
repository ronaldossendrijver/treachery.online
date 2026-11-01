/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Concurrent;
using System.Reflection;

namespace Treachery.Bots;

public enum BotType
{
    Basic = 0,
    Advanced = 1
}

/// <summary>
/// Provides bots of a specific type. Implementations are discovered in plugin assemblies (Treachery.Bots.*.dll) by <see cref="BotFactory.LoadPlugins"/>
/// and need a public parameterless constructor.
/// </summary>
public interface IBotProvider
{
    BotType Type { get; }

    IBot Create(Game game, Player player);
}

public static class BotFactory
{
    private const string PluginPattern = "Treachery.Bots.*.dll";

    private static readonly ConcurrentDictionary<BotType, IBotProvider> Providers = new();

    public static void Register(IBotProvider provider) => Providers[provider.Type] = provider;

    public static void Unregister(BotType type) => Providers.TryRemove(type, out _);

    public static bool IsAvailable(BotType type) => type == BotType.Basic || Providers.ContainsKey(type);

    /// <summary>
    /// The type of bot the game asks for. Bots are of the same type for all seats.
    /// </summary>
    public static BotType BotTypeFor(Game game) => game.Applicable(Rule.AdvancedBots) ? BotType.Advanced : BotType.Basic;

    /// <summary>
    /// Creates a bot of the given type, or a BasicBot if that type is not available on this server.
    /// </summary>
    public static IBot Create(BotType type, Game game, Player player, BotParameters? parameters = null)
    {
        if (type != BotType.Basic && Providers.TryGetValue(type, out var provider))
            return provider.Create(game, player);

        return new BasicBot(game, player, parameters ?? BotParameters.GetDefaultParameters(player.Faction));
    }

    /// <summary>
    /// Loads bot plugin assemblies (Treachery.Bots.*.dll) from the given directory and registers their bot providers.
    /// Returns a description of each loaded or failed plugin.
    /// </summary>
    public static IReadOnlyList<string> LoadPlugins(string directory)
    {
        var result = new List<string>();
        if (!Directory.Exists(directory)) return result;

        foreach (var file in Directory.GetFiles(directory, PluginPattern))
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);
                foreach (var type in assembly.GetExportedTypes().Where(t => typeof(IBotProvider).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false }))
                {
                    if (Activator.CreateInstance(type) is not IBotProvider provider) continue;
                    Register(provider);
                    result.Add($"Bot plugin loaded: {provider.Type} bots from {Path.GetFileName(file)} (build {BuildOf(assembly)})");
                }
            }
            catch (Exception e)
            {
                result.Add($"Bot plugin {Path.GetFileName(file)} could not be loaded: {e.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// The build number of an assembly: its assembly version (just the major number when the rest is zero),
    /// falling back to its informational version without source revision suffix.
    /// </summary>
    public static string BuildOf(Assembly assembly)
    {
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var version = assembly.GetName().Version;
        if (version != null && version.Major > 0)
            return version.Minor == 0 && version.Build <= 0 && version.Revision <= 0 ? version.Major.ToString() : version.ToString();

        return string.IsNullOrWhiteSpace(informational) ? "unknown" : informational.Split('+')[0];
    }
}