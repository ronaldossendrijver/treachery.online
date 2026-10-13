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

        var header = first.Find(".ui-card-header");
        if (first.FindAll(".ui-card-body").Count == 0)
        {
            header.Click();
        }

        header.Click();
        Assert.IsEmpty(first.FindAll(".ui-card-body"));

        using var secondContext = new ComponentTestContext();
        var second = RenderLayout(secondContext);
        Assert.IsEmpty(second.FindAll(".ui-card-body"));
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
