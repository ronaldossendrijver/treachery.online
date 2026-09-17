using Bunit;
using NSubstitute;
using Treachery.Client.OtherComponents;
using Treachery.Shared;

namespace Treachery.Client.Test;

[TestClass]
[DoNotParallelize]
public sealed class ActionPanelTests
{
    [TestMethod]
    public void SimpleActionMappingRendersAndSubmits()
    {
        using var test = new ComponentTestContext();
        AllianceBroken? submitted = null;
        test.Client.Actions.Returns([typeof(AllianceBroken)]);
        test.Client.IsObserver.Returns(true);
        test.Client.RequestGameEvent(Arg.Do<AllianceBroken>(value => submitted = value))
            .Returns(Task.FromResult<string?>(null));

        var component = test.RenderContext.Render<ActionPanel>();

        Assert.Contains("Break your current alliance?", component.Markup);
        Assert.AreEqual("Break", component.Find("button").TextContent);
        Assert.IsTrue(component.Find("button").HasAttribute("disabled"));
        Assert.IsNull(submitted);
    }
}
