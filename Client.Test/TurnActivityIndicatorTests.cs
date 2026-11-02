using System.Reflection;
using Treachery.Client.OtherComponents;

namespace Treachery.Client.Test;

[TestClass]
public sealed class TurnActivityIndicatorTests
{
    [TestMethod]
    [DataRow(6, true, 6, 34)]
    [DataRow(7, true, 4, 34)]
    [DataRow(12, true, 6, 34)]
    [DataRow(7, false, 4, 21)]
    [DataRow(12, false, 6, 21)]
    public void GridKeepsBiddingLogosVisibleBehindLargerCounts(int count, bool showCardCounts, int columns, int size)
    {
        var method = typeof(TurnActivityIndicator).GetMethod("GridStyle", BindingFlags.Static | BindingFlags.NonPublic)!;

        var style = (string)method.Invoke(null, [count, showCardCounts])!;

        Assert.AreEqual($"--turn-grid-columns:{columns};--turn-grid-size:{size}px", style);
    }
}
