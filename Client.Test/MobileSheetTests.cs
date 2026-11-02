using System.Reflection;
using Microsoft.AspNetCore.Components.Web;
using Treachery.Client.Pages;

namespace Treachery.Client.Test;

[TestClass]
public sealed class MobileSheetTests
{
    [TestMethod]
    public void VerticalSwipesOpenAndCloseWithoutSyntheticClickTogglingAgain()
    {
        var page = new Main();
        Start(page, 100, 100);
        End(page, 100, 60);
        Assert.IsTrue(Expanded(page));
        Click(page);
        Assert.IsTrue(Expanded(page));

        Start(page, 100, 60);
        End(page, 100, 100);
        Assert.IsFalse(Expanded(page));
        Click(page);
        Assert.IsFalse(Expanded(page));
    }

    [TestMethod]
    public void TapsAndKeyboardActivationStillToggle()
    {
        var page = new Main();
        Start(page, 100, 100);
        End(page, 102, 105);
        Click(page);
        Assert.IsTrue(Expanded(page));
        Click(page, 0);
        Assert.IsFalse(Expanded(page));

        Start(page, 100, 100);
        End(page, 100, 60);
        Click(page, 0);
        Assert.IsFalse(Expanded(page), "A swipe must not suppress keyboard activation.");
    }

    [TestMethod]
    public void SwipeRequiresThirtyPixelsAndPredominantlyVerticalDirection()
    {
        var page = new Main();
        Start(page, 100, 100);
        End(page, 100, 71);
        Assert.IsFalse(Expanded(page));
        Start(page, 100, 100);
        End(page, 150, 60);
        Assert.IsFalse(Expanded(page));
        Start(page, 100, 100);
        End(page, 100, 70);
        Assert.IsTrue(Expanded(page));
    }

    [TestMethod]
    public void CancelledAndMultiTouchGesturesDoNotToggle()
    {
        var page = new Main();
        Start(page, 100, 100);
        Invoke(page, "CancelSheetSwipe");
        End(page, 100, 50);
        Assert.IsFalse(Expanded(page));

        Invoke(page, "StartSheetSwipe", new TouchEventArgs
        {
            Touches = [Point(100, 100), Point(120, 100)]
        });
        End(page, 100, 50);
        Assert.IsFalse(Expanded(page));
    }

    private static TouchPoint Point(double x, double y) => new() { ClientX = x, ClientY = y };

    private static void Start(Main page, double x, double y) =>
        Invoke(page, "StartSheetSwipe", new TouchEventArgs { Touches = [Point(x, y)] });

    private static void End(Main page, double x, double y) =>
        Invoke(page, "EndSheetSwipe", new TouchEventArgs { ChangedTouches = [Point(x, y)] });

    private static void Click(Main page, long detail = 1) =>
        Invoke(page, "TogglePhoneSheet", new MouseEventArgs { Detail = detail });

    private static bool Expanded(Main page) =>
        (bool)typeof(Main).GetField("_phoneSheetExpanded", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page)!;

    private static void Invoke(Main page, string method, params object[] args) =>
        typeof(Main).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, args);
}
