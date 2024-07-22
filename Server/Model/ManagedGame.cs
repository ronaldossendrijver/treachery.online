using System.Collections.Generic;
using Treachery.Shared;

namespace Treachery.Server;

public class ManagedGame
{
    public Game Game { get; set; }

    public List<int> Players { get; set; } = [];

    public List<int> Observers { get; set; } = [];

    public List<int> Hosts { get; set; } = [];

    public string HashedPassword { get; set; }

    public bool IsHost(int playerId) => Hosts.Contains(playerId);

    public bool HasRoomFor(Faction faction)
    {
        //TODO implement
        return true;
    }
}