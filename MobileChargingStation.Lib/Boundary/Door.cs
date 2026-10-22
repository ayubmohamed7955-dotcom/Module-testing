using MobileChargingStation.Lib.Interfaces;

namespace MobileChargingStation.Lib.Boundary;

/// <summary>
/// Simuleret låge. OnDoorOpen/OnDoorClose kaldes af "hardwaren" (her: konsol-appen)
/// og omsættes til events, som StationControl abonnerer på.
/// </summary>
public class Door : IDoor
{
    public event EventHandler? DoorOpenedEvent;
    public event EventHandler? DoorClosedEvent;

    public bool IsOpen { get; private set; }
    public bool IsLocked { get; private set; }

    public void OnDoorOpen()
    {
        // En låst dør kan ikke åbnes, og en åben dør kan ikke åbnes igen
        if (IsLocked || IsOpen) return;

        IsOpen = true;
        DoorOpenedEvent?.Invoke(this, EventArgs.Empty);
    }

    public void OnDoorClose()
    {
        if (!IsOpen) return;

        IsOpen = false;
        DoorClosedEvent?.Invoke(this, EventArgs.Empty);
    }

    public void LockDoor()
    {
        IsLocked = true;
    }

    public void UnlockDoor()
    {
        IsLocked = false;
    }
}
