namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public class OwnGamesProfileValidationTests
{
    [TestMethod]
    public void ProfileEditorShowsServerErrorAndPreservesEnteredValues()
    {
        using var test = CreateContext();
        test.Client.RequestUpdateUserInfo("", "changed@example.com", "Changed player")
            .Returns(Task.FromResult<string?>("Enter a valid e-mail address"));
        var component = test.RenderContext.Render<OwnGamesComponent>();
        component.FindAll("button").Single(button => button.TextContent == "Change account info").Click();
        component.Find("#playerName").Change("Changed player");
        component.Find("#email").Change("changed@example.com");
        component.FindAll("button").Single(button => button.TextContent == "Apply changes").Click();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Enter a valid e-mail address", component.Markup);
            Assert.IsFalse(component.Find("#playerName").HasAttribute("readonly"));
            Assert.AreEqual("Changed player", component.Find("#playerName").GetAttribute("value"));
            Assert.AreEqual("changed@example.com", component.Find("#email").GetAttribute("value"));
        });
    }

    [TestMethod]
    [DataRow("abc", ErrorType.PlayerNameTooShort)]
    [DataRow("12345678901234567890123456789012345678901", ErrorType.PlayerNameTooLong)]
    public void ProfileEditorRejectsInvalidPlayerNames(string playerName, ErrorType error)
    {
        using var test = CreateContext();
        var component = test.RenderContext.Render<OwnGamesComponent>();
        component.FindAll("button").Single(button => button.TextContent == "Change account info").Click();
        component.Find("#playerName").Change(playerName);
        component.FindAll("button").Single(button => button.TextContent == "Apply changes").Click();

        Assert.Contains(DefaultSkin.Default.Describe(error), component.Markup);
        test.Client.DidNotReceiveWithAnyArgs().RequestUpdateUserInfo(default!, default!, default!);
        Assert.IsFalse(component.Find("#playerName").HasAttribute("readonly"));
    }

    [TestMethod]
    public void ProfileEditorClosesAfterSuccessfulSave()
    {
        using var test = CreateContext();
        test.Client.RequestUpdateUserInfo(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult<string?>(null));
        var component = test.RenderContext.Render<OwnGamesComponent>();
        component.FindAll("button").Single(button => button.TextContent == "Change account info").Click();
        component.FindAll("button").Single(button => button.TextContent == "Apply changes").Click();

        component.WaitForAssertion(() => Assert.IsTrue(component.Find("#playerName").HasAttribute("readonly")));
        test.Client.Received(1).RequestUpdateUserInfo("", "player@example.com", "Player");
    }

    private static ComponentTestContext CreateContext()
    {
        var test = new ComponentTestContext();
        test.Client.OwnGames.Returns([]);
        test.Client.ScheduledGames.Returns([]);
        test.Client.PlayerName.Returns("Player");
        test.Client.UserEmail.Returns("player@example.com");
        test.Client.UserId.Returns(1);
        return test;
    }
}
