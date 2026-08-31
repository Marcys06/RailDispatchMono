namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    private bool _radioStopActive;

    /// <summary>
    /// RadioStop is the first-generation train stop command. At this stage it
    /// only forces the train to zero speed. The command is intentionally kept
    /// separate so it can become an event/dispatcher command later.
    /// </summary>
    public void RadioStop()
    {
        _radioStopActive = true;
        Speed = 0f;
    }

    public void ClearRadioStop() => _radioStopActive = false;

    public bool IsRadioStopped => _radioStopActive;
}
