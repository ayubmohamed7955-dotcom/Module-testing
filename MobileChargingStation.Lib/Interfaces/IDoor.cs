namespace MobileChargingStation.Lib.Interfaces;

public interface IDoor
{
    event EventHandler? DoorOpenedEvent;
    event EventHandler? DoorClosedEvent;

    void LockDoor();
    void UnlockDoor();
}