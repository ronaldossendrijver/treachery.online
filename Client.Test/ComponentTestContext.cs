using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Treachery.Shared;

namespace Treachery.Client.Test;

internal sealed class ComponentTestContext : IDisposable
{
    public BunitContext RenderContext { get; } = new();

    public IGameService Client { get; }

    public ComponentTestContext(Game? game = null, Faction faction = Faction.Green)
    {
        game ??= new Game();

        Message.DefaultDescriber = DefaultSkin.Default;
        RenderContext.JSInterop.Mode = JSRuntimeMode.Loose;

        Client = Substitute.For<IGameService>();
        Client.Game.Returns(game);
        Client.Player.Returns(game.GetPlayer(faction));
        Client.Faction.Returns(faction);
        Client.CurrentPhase.Returns(_ => Client.Game?.CurrentPhase);
        Client.CurrentSkin.Returns(DefaultSkin.Default);
        Client.Actions.Returns([]);

        RenderContext.Services.AddSingleton(Client);
        RenderContext.Services.AddSingleton(new Browser(RenderContext.JSInterop.JSRuntime));
    }

    public void Dispose()
    {
        RenderContext.Dispose();
    }
}
