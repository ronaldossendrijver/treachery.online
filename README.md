# Threachery.online

Welcome to treachery.online!

# Quick overview of the software
When someone visits https://treachery.online, the client software (found in namespace Treachery.online.Client, written in Blazor/C#) is downloaded to the browser as a "Blazor WebAssembly binary" and executed in the browser in a single page application. Both the user interface and the game itself, with its state and rules, are executed on the client side. When a client starts a new game, it will also act as a host that other players can communicate with. The communication between clients is done via a Server (found in namespace Treachery.online.Server, written in C#) that runs on the webserver. The Server does nothing more than relaying messages (primarely GameEvent objects) between the Host-client and the other clients. Namespace Threachery.online.Shared, written in C#, contains the "core" of the game, the most important class being Game. The Game class contains all the logic of the game: the rules and the current state of the Game.

# GameEvents
Each action a player can do in the game, like selecting a traitor or finalizing a battle plan, is a specific subclass of the GameEvent object. When a player for example selects a traitor and presses "Ok", the flow of events is as follows:
1. The player client constructs a TraitorSelected event-object (a subclass of GameEvent)
2. The clients sends a request to execute this event to the server
3. The server forwards the request to the Host client
4. The Host client checks if the event if valid by executing it against its local Game object
5. In case the event is valid, a confirmation message is sent to the server
6. The server forwards the confirmation message to all client (including the client where the host runs)
7. The event is executed against each client's local Game object
8. The event has now been executed and all clients are in sync!
