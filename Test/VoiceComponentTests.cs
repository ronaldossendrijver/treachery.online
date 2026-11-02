using Bunit;
using NSubstitute;
using Treachery.Client.GameEventComponents;
using Treachery.Client.GenericComponents;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class VoiceComponentTests
{
    [TestMethod]
    [DataRow(Faction.Blue, false)]
    [DataRow(Faction.White, false)]
    [DataRow(Faction.Blue, true)]
    [DataRow(Faction.White, true)]
    public void PassSubmitsIndependentlyOfCardTypeSelection(Faction faction, bool noCardType)
    {
        var game = VoiceTests.CreateBattle(faction);
        using var test = new ComponentTestContext(game, faction);
        Voice? submitted = null;
        test.Client.RequestGameEvent(Arg.Do<Voice>(value => submitted = value))
            .Returns(Task.FromResult<string?>(null));
        if (noCardType)
            test.RenderContext.ComponentFactories.AddStub<SelectComponent<TreacheryCardType>>();
        var component = test.RenderContext.Render<VoiceComponent>();

        var pass = component.FindAll("button").Single(button => button.TextContent.Trim() == "Pass");
        var confirm = component.FindAll("button").Single(button => button.TextContent.Trim() == "Confirm");
        Assert.IsFalse(pass.HasAttribute("disabled"));
        Assert.AreEqual(noCardType, confirm.HasAttribute("disabled"));
        Assert.Contains("Use or pass Voice before Prescience", component.Markup);

        pass.Click();

        Assert.IsNotNull(submitted);
        Assert.AreSame(game, submitted.Game);
        Assert.AreEqual(faction, submitted.Initiator);
        Assert.IsTrue(submitted.Passed);
        Assert.IsNull(submitted.Validate());
    }

    [TestMethod]
    [DataRow(Faction.Blue)]
    [DataRow(Faction.White)]
    public void ConfirmStillSubmitsARealVoiceDecision(Faction faction)
    {
        var game = VoiceTests.CreateBattle(faction);
        using var test = new ComponentTestContext(game, faction);
        Voice? submitted = null;
        test.Client.RequestGameEvent(Arg.Do<Voice>(value => submitted = value))
            .Returns(Task.FromResult<string?>(null));
        var component = test.RenderContext.Render<VoiceComponent>();

        component.Find(".ui-btn-confirm").Click();

        Assert.IsNotNull(submitted);
        Assert.IsFalse(submitted.Passed);
        Assert.IsTrue(submitted.Must);
        Assert.Contains(submitted.Type, Voice.ValidTypes(game));
        Assert.IsNull(submitted.Validate());
    }
}
