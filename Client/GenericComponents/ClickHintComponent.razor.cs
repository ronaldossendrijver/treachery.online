namespace Treachery.Client.GenericComponents;

[Flags]
public enum ClickHintButton
{
    None = 0,
    Lmb = 1,
    Rmb = 2
}

[Flags]
public enum ClickHintModifier
{
    None = 0,
    Shift = 1,
    Ctrl = 2
}