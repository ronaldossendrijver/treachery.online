using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
public sealed partial class ActionPanelInventoryTests
{
    private static readonly HashSet<string> ActionsRenderedOutsideSwitch =
    [
        nameof(Bid),
        nameof(BlackMarketBid),
        nameof(DealAccepted),
        nameof(EndPhase),
        nameof(PlayerReplaced)
    ];

    [TestMethod]
    public void ActionPanelMappingsReferToGameEvents()
    {
        var root = FindRepositoryRoot();
        var actionPanelSource = File.ReadAllText(Path.Combine(root, "Client", "OtherComponents", "ActionPanel.razor"));
        var eventTypes = typeof(GameEvent).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(GameEvent)))
            .Select(type => type.Name)
            .ToHashSet();

        var unknown = ActionPanelCaseRegex().Matches(actionPanelSource)
            .Select(match => match.Groups[1].Value)
            .Where(name => !eventTypes.Contains(name))
            .Distinct()
            .Order()
            .ToArray();

        Assert.IsEmpty(
            unknown,
            $"ActionPanel mappings that are not concrete GameEvent types: {string.Join(", ", unknown)}");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "treachery.online.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not find the repository root.");
    }

    [GeneratedRegex(@"result\.Add\(typeof\((\w+)\)\)")]
    private static partial Regex ApplicableEventRegex();

    [GeneratedRegex(@"case nameof\((\w+)\):")]
    private static partial Regex ActionPanelCaseRegex();
}
