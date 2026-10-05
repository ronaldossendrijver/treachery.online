using System.Runtime.CompilerServices;
using Treachery.Client;

namespace Treachery.Client.Test;

[TestClass]
public sealed class LocationClickTests
{
    [TestMethod]
    public void LocationClickIgnoresNullEventArgs()
    {
        var client = (Client)RuntimeHelpers.GetUninitializedObject(typeof(Client));

        client.LocationClick(null);
    }
}
