using Bunit;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using Treachery.Client.GameEventComponents;
using Treachery.Client.GenericComponents;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class SharedComponentTests
{
    [TestMethod]
    public void SimpleActionConfirmSubmitsInitializedEvent()
    {
        using var test = new ComponentTestContext();
        ValidTestEvent? submitted = null;
        test.Client.RequestGameEvent(Arg.Do<ValidTestEvent>(value => submitted = value))
            .Returns(Task.FromResult<string?>(null));

        var component = test.RenderContext.Render<SimpleActionComponent<ValidTestEvent>>(parameters => parameters
            .Add(value => value.Title, "Perform action")
            .Add(value => value.ConfirmLabel, "Do it"));

        component.FindAll("button").Single(button => button.TextContent == "Do it").Click();

        Assert.IsNotNull(submitted);
        Assert.AreSame(test.Client.Game, submitted.Game);
        Assert.AreEqual(Faction.Green, submitted.Initiator);
    }

    [TestMethod]
    public void SimpleActionShowsValidationAndPreventsSubmission()
    {
        using var test = new ComponentTestContext();

        var component = test.RenderContext.Render<SimpleActionComponent<InvalidTestEvent>>(parameters => parameters
            .Add(value => value.Title, "Invalid action"));

        var button = component.Find("button");
        Assert.IsTrue(button.HasAttribute("disabled"));
        Assert.AreEqual("Choose a value", button.ParentElement?.GetAttribute("title"));
    }

    [TestMethod]
    public void DismissRemovesSimpleActionWithoutSubmitting()
    {
        using var test = new ComponentTestContext();

        var component = test.RenderContext.Render<SimpleActionComponent<ValidTestEvent>>(parameters => parameters
            .Add(value => value.Title, "Dismissable action")
            .Add(value => value.Dismissable, true));

        component.FindAll("button").Single(button => button.TextContent == "Dismiss").Click();

        Assert.DoesNotContain("Dismissable action", component.Markup);
        test.Client.DidNotReceiveWithAnyArgs().RequestGameEvent(default(ValidTestEvent)!);
    }

    [TestMethod]
    [DataRow("Yes", false)]
    [DataRow("No", true)]
    public void YesNoActionSubmitsExpectedPassedValue(string buttonText, bool expectedPassed)
    {
        using var test = new ComponentTestContext();
        PassableTestEvent? submitted = null;
        test.Client.RequestGameEvent(Arg.Do<PassableTestEvent>(value => submitted = value))
            .Returns(Task.FromResult<string?>(null));

        var component = test.RenderContext.Render<YesNoActionComponent<PassableTestEvent>>(parameters => parameters
            .Add(value => value.Title, "Choose"));

        component.FindAll("button").Single(button => button.TextContent == buttonText).Click();

        Assert.IsNotNull(submitted);
        Assert.AreEqual(expectedPassed, submitted.Passed);
        Assert.AreEqual(Faction.Green, submitted.Initiator);
    }

    [TestMethod]
    public void CheckboxRaisesBindingAndChangeCallbacks()
    {
        using var test = new ComponentTestContext();
        bool? boundValue = null;
        bool? changedValue = null;

        var component = test.RenderContext.Render<CheckboxComponent>(parameters => parameters
            .Add(value => value.Value, false)
            .Add(value => value.ValueChanged, EventCallback.Factory.Create<bool>(this, value => boundValue = value))
            .Add(value => value.OnChanged, EventCallback.Factory.Create<bool>(this, value => changedValue = value)));

        component.Find("input").Change(true);

        Assert.IsTrue(boundValue);
        Assert.IsTrue(changedValue);
    }

    [TestMethod]
    public void SelectRejectsUnknownValues()
    {
        using var test = new ComponentTestContext();
        var selected = 0;

        var component = test.RenderContext.Render<SelectComponent<int>>(parameters => parameters
            .Add(value => value.Values, [1, 2])
            .Add(value => value.Value, 1)
            .Add(value => value.ValueChanged, EventCallback.Factory.Create<int>(this, value => selected = value)));

        component.Find("select").Change("99");

        Assert.AreEqual(0, selected);
    }
}
