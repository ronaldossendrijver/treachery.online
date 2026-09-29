# Treachery.online

Welcome to treachery.online!

# Quick overview of the software
When someone visits https://treachery.online, the client software (found in namespace treachery.online.Client, written in Blazor/C#) is downloaded to the browser as a "Blazor WebAssembly binary" and executed in the browser in a single page application. Both the user interface and the game itself, with its state and rules, are executed on the client side. 

When someone starts a new game, it is hosted on the webserver (found in namespace treachery.online.Server, written in C#), which is a SignalR hub. All connected players communicate with that hub.

Namespace treachery.online.Shared, written in C#, contains the "core" of the game, the most important class being Game (divided into). The Game class contains all the logic of the game: the rules and the current state of the Game.

# GameEvents
Each action a player can do in the game, like selecting a traitor or finalizing a battle plan, is a specific subclass of GameEvent. When a player for example selects a traitor and presses "Ok", the flow of events is as follows:
1. The player client constructs a TraitorSelected object (a subclass of GameEvent)
2. The clients sends a request to execute this event to the server
3. The server checks if the event if valid by executing it against its local Game object
4. In case the event is valid, a request to handle the event is sent to all clients (including to the client that initiated the event)
5. The event is executed against each client's local Game object
6. The event has now been executed and all clients are in sync!

# User Interface
Namespace treachery.online.Client holds the UI of the application. It is a Blazor Webassembly app running in the Browser. It is basically a view on the client's local Game object and changes with it. 

Account creation and profile updates are validated on the server before saving: usernames and player names must contain 4-40 characters after trimming, email addresses must be valid bare addresses no longer than 254 characters, and password hashes must be 64 hexadecimal characters (SHA-256). An empty password on a profile update retains the current password. Game names are limited to 128 characters and game password hashes are optional, but otherwise use the same SHA-256 format. Existing oversized player names must be corrected before creating, scheduling, joining, or observing a new game. The profile editor displays rejected updates without discarding the entered values. SQLite does not enforce the model's string-length annotations; these checks are explicit at the hub entry points. Serialized game state and participation remain uncapped by these short-field limits.

When playing a game, the UI is divided into 3 sections: the Map, an Actions section showing the GameEvents that can be executed by the player based on the local Game state and an Information-section, containing what is behing the player shield and the Log. An Observer has the same sections, except for the Actions section.
