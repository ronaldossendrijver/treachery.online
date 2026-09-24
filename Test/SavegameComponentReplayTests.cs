using System.IO;
using System.Reflection;
using System.Threading;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NSubstitute;
using Treachery.Client.GenericComponents;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class SavegameComponentReplayTests
{
    private static readonly Type[] EventsWithoutGameEventComponents =
    [
        typeof(Bid),
        typeof(BlackMarketBid),
        typeof(DealAccepted),
        typeof(EndPhase),
        typeof(PlayerReplaced)
    ];

    [TestMethod]
    [TestCategory("Savegame")]
    [Timeout(60 * 60 * 1000, CooperativeCancellation = true)]
    [DoNotParallelize]
    public void LatestSavegameVersionReproduceHistoricalComponentEvents()
    {
        Console.WriteLine("Re-playing all savegame files in {0}...", Directory.GetCurrentDirectory());
        
        var files = GetSavegameFiles();

        var testedEvents = 0;
        ParallelOptions po = new()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
        Parallel.ForEach(files, po, file =>
        {
            var state = GameState.Load(File.ReadAllText(file));
            if (state.Version < Game.LatestVersion - 3)
            {
                return;
            }

            Console.WriteLine("Checking {0} (version {1})...", file, state.Version);
            
            var game = CurrentVersionGameFixture.Create(state.Version);
            var eventIndex = 0;
            foreach (var historicalEvent in state.Events)
            {
                try
                {
                    historicalEvent.Initialize(game);
                    var context = $"{Path.GetFileName(file)}, event {eventIndex}, {historicalEvent.GetType().Name}";
                    var historicalValidation = historicalEvent.Validate();
                    Assert.IsNull(
                        historicalValidation,
                        $"{context}: the historical event is not valid in its reconstructed pre-event state: {historicalValidation?.ToString(DefaultSkin.Default)}");

                    if (HasConfirmableGameEventComponent(historicalEvent))
                    {
                        ReproduceComponentEvent(game, historicalEvent, context);
                        Interlocked.Increment(ref testedEvents);
                    }

                    var executionError = historicalEvent.Execute(false, true);
                    Assert.IsNull(
                        executionError,
                        $"{context}: historical event could not be replayed: {executionError?.ToString(DefaultSkin.Default)}");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(
                        "Exception while replaying {0}, event {1} ({2}), faction {3}: {4}",
                        Path.GetFileName(file),
                        eventIndex,
                        historicalEvent.GetType().Name,
                        historicalEvent.Initiator,
                        exception.Message.Split('\n', 2)[0].TrimEnd());
                    Console.WriteLine(exception.StackTrace);
                    throw;
                }

                eventIndex++;
            }
        });

        Assert.IsGreaterThan(0, testedEvents);
    }

    private static bool HasConfirmableGameEventComponent(GameEvent gameEvent)
    {
        if (EventsWithoutGameEventComponents.Contains(gameEvent.GetType()))
        {
            return false;
        }

        return gameEvent is not AutomationConfigured { Action: not ItemAction.Create }
               && gameEvent is not DealOffered { Cancel: true }
               && (gameEvent is not ClairVoyancePlayed clairvoyance ||
                   IsRepresentable(clairvoyance));
    }

    private static bool IsRepresentable(ClairVoyancePlayed clairvoyance)
    {
        if (!ClairVoyancePlayed.ValidQuestions(
                clairvoyance.Game,
                clairvoyance.Target)
            .Contains(clairvoyance.Question))
        {
            return false;
        }

        return clairvoyance.Question is not (
                   ClairvoyanceQuestion.CardTypeInBattle or
                   ClairvoyanceQuestion.CardTypeAsDefenseInBattle or
                   ClairvoyanceQuestion.CardTypeAsWeaponInBattle or
                   ClairvoyanceQuestion.HasCardTypeInHand)
               || clairvoyance.Parameter1 is TreacheryCardType cardType &&
               ClairVoyancePlayed.ValidCardTypes(clairvoyance.Game, clairvoyance.Question).Contains(cardType);
    }

    private static void ReproduceComponentEvent(
        Game game,
        GameEvent historicalEvent,
        string context)
    {
        using var test = new ComponentTestContext(game, historicalEvent.Initiator);
        test.Client.IsHost.Returns(true);
        test.Client.IsObserver.Returns(true);
        test.Client.Actions.Returns([historicalEvent.GetType()]);
        if (historicalEvent is FactionSelected factionSelected)
        {
            test.Client.PlayerName.Returns(factionSelected.InitiatorPlayerName);
            test.Client.Player.Returns(
                game.Players.Single(player => player.Seat == factionSelected.Seat));
        }

        IRenderedComponent<DynamicComponent> rendered;
        try
        {
            rendered = HistoricalComponentDriver.Render(test, historicalEvent);
        }
        catch (Exception exception)
        {
            Assert.Fail($"{context}: component raised a Blazor rendering error.{Environment.NewLine}{exception}");
            return;
        }

        if (rendered.FindAll(".card-body").Count == 0)
        {
            rendered.Find(".card-header").Click();
        }

        var postRenderHistoricalValidation = historicalEvent.Validate();
        Assert.IsNull(
            postRenderHistoricalValidation,
            $"{context}: rendering the component changed the game so the historical event is no longer valid: {postRenderHistoricalValidation?.ToString(DefaultSkin.Default)}");
        var renderExceptions = rendered.FindComponents<ErrorBoundary>()
            .Select(boundary => GetErrorBoundaryException(boundary.Instance))
            .OfType<Exception>()
            .ToArray();
        Assert.IsEmpty(
            renderExceptions,
            $"{context}: component raised a Blazor rendering error.{Environment.NewLine}{Utilities.Serialize<GameEvent>(historicalEvent)}{Environment.NewLine}{string.Join(Environment.NewLine, renderExceptions.Select(exception => exception.ToString()))}");

        HistoricalComponentDriver.AssertInputsAreRepresentable(rendered);
        if (rendered.Instance.Instance is not IComponent component)
        {
            throw new InvalidOperationException($"{context}: component was not instantiated.");
        }
        if (historicalEvent is AcceptOrCancelPurpleRevival
            {
                Cancel: true,
                Hero: { } cancelledHero
            })
        {
            var offerIndex = game.EarlyRevivalsOffers.Keys.ToList()
                .FindIndex(hero => hero.Id == cancelledHero.Id);
            Assert.IsGreaterThanOrEqualTo(
                0,
                offerIndex,
                $"{context}: the historical revival offer was not rendered.");
            var cancelButton = rendered.FindAll("button")
                .Where(button => button.TextContent.Trim() == "Cancel")
                .ElementAt(offerIndex);
            Assert.IsFalse(
                cancelButton.HasAttribute("disabled"),
                $"{context}: the Cancel button was disabled.");
            cancelButton.Click();
            AssertSubmittedEvent(test, historicalEvent, context, "Cancel");
            return;
        }

        var componentResult = HistoricalComponentDriver.ReadResult(
            component,
            historicalEvent);
        var actualEvent = componentResult.GameEvent
            ?? throw new InvalidOperationException($"{context}: component result was null.");

        var validation = actualEvent.Validate();
        Assert.IsNull(
            validation,
            $"{context}: historical inputs did not enable submission: {validation?.ToString(DefaultSkin.Default)}{Environment.NewLine}{Utilities.Serialize<GameEvent>(actualEvent)}");

        var buttonRow = rendered.FindComponents<ButtonRowComponent>().LastOrDefault();
        var buttonText = historicalEvent switch
        {
            GreyRemovedCardFromAuction { PutOnTop: false } => "Put at bottom",
            _ when buttonRow == null
                => historicalEvent switch
                {
                    AutomationConfigured => "Create rule",
                    AllyPermission => "Confirm Changes",
                    Move when componentResult.PropertyName == "PassedResult" => "Pass",
                    Move => "Perform a Move",
                    _ => throw new InvalidOperationException(
                        $"{context}: no submission button row was rendered.")
                },
            _ => componentResult.PropertyName switch
            {
                "PassedResult" => buttonRow!.Instance.PassText,
                "OtherResult" => buttonRow!.Instance.OtherText,
                _ => buttonRow!.Instance.ConfirmText
            }
        };
        var availableButtons = buttonRow?.FindAll("button") ?? rendered.FindAll("button");
        var matchingButtons = availableButtons
            .Where(button => button.TextContent.Trim() == buttonText)
            .ToArray();
        Assert.HasCount(
            1,
            matchingButtons,
            $"{context}: expected one {buttonText} button; rendered buttons: {string.Join(", ", availableButtons.Select(button => button.TextContent.Trim()))}{Environment.NewLine}{rendered.Markup}");
        var submitButton = matchingButtons[0];
        Assert.IsFalse(
            submitButton.HasAttribute("disabled"),
            $"{context}: the {buttonText} button was disabled: {submitButton.ParentElement?.GetAttribute("title")}");

        submitButton.Click();
        AssertSubmittedEvent(test, historicalEvent, context, buttonText);
    }

    private static void AssertSubmittedEvent(
        ComponentTestContext test,
        GameEvent historicalEvent,
        string context,
        string buttonText)
    {
        var submittedEvent = test.Client.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name ==
                           nameof(IGameService.RequestGameEvent))
            .SelectMany(call => call.GetArguments())
            .OfType<GameEvent>()
            .LastOrDefault();
        Assert.IsNotNull(
            submittedEvent,
            $"{context}: clicking {buttonText} did not submit a game event.");
        HistoricalGameEventComparer.AssertEquivalent(
            historicalEvent,
            submittedEvent,
            context);
    }

    private static string[] GetSavegameFiles()
    {
        var files = Directory.EnumerateFiles(".", "savegame*.json").ToArray();

        if (files.Length == 0)
        {
            Assert.Inconclusive(
                "No savegames found in test directory. Copy the corpus there before running Savegame tests.");
        }

        var limitText = Environment.GetEnvironmentVariable("TREACHERY_SAVEGAME_LIMIT");
        if (!int.TryParse(limitText, out var limit) || limit <= 0)
        {
            return files;
        }

        var filesWithVersions = files
            .Select(file => (File: file, Version: GameState.Load(File.ReadAllText(file)).Version))
            .ToArray();
        var versions = filesWithVersions.Select(item => item.Version)
            .Distinct()
            .OrderDescending()
            .Take(2);
        return versions.SelectMany(version => filesWithVersions
                .Where(item => item.Version == version)
                .OrderBy(item => item.File)
                .Take(limit)
                .Select(item => item.File))
            .ToArray();
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

    private static Exception? GetErrorBoundaryException(ErrorBoundary boundary)
    {
        var property = typeof(ErrorBoundaryBase).GetProperty(
            "CurrentException",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        return property?.GetValue(boundary) as Exception;
    }
}
