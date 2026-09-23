using Bunit;
using Microsoft.AspNetCore.Components;
using Treachery.Client.GenericComponents;

namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class GameEventComponentLayoutTests
{
    private sealed class PersistedCollapseMarker;

    [TestMethod]
    public void CollapseStateIsRememberedForComponentType()
    {
        using var firstContext = new ComponentTestContext();
        var first = RenderLayout(firstContext);

        first.Find(".card-header").Click();
        Assert.IsEmpty(first.FindAll(".card-body"));

        using var secondContext = new ComponentTestContext();
        var second = RenderLayout(secondContext);
        Assert.IsEmpty(second.FindAll(".card-body"));
    }

    [TestMethod]
    public void UrgentLayoutUsesVisibleBorder()
    {
        using var test = new ComponentTestContext();

        var component = test.RenderContext.Render<GameEventComponentLayout>(parameters => parameters
            .Add(value => value.IsUrgent, true)
            .Add(value => value.Collapsible, false)
            .Add(value => value.Body, Body("Body")));

        Assert.Contains("border-white", component.Find(".card").ClassList);
    }

    private static IRenderedComponent<GameEventComponentLayout> RenderLayout(ComponentTestContext test)
    {
        return test.RenderContext.Render<GameEventComponentLayout>(parameters => parameters
            .Add(value => value.CollapsedType, typeof(PersistedCollapseMarker))
            .Add(value => value.Header, Body("Header"))
            .Add(value => value.Body, Body("Body")));
    }

    private static RenderFragment Body(string text) => builder => builder.AddContent(0, text);
}
