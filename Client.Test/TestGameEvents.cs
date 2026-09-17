using Treachery.Shared;

namespace Treachery.Client.Test;

public sealed class ValidTestEvent : GameEvent
{
    public override Message? Validate() => null;

    protected override void ExecuteConcreteEvent()
    {
    }
}

public sealed class InvalidTestEvent : GameEvent
{
    public override Message? Validate() => Message.Express("Choose a value");

    protected override void ExecuteConcreteEvent()
    {
    }
}

public sealed class PassableTestEvent : PassableGameEvent
{
    public override Message? Validate() => null;

    protected override void ExecuteConcreteEvent()
    {
    }
}
