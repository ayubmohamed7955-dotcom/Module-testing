using MobileChargingStation.Lib.Interfaces;

namespace MobileChargingStation.Lib.Boundary;

/// <summary>
/// Simuleret RFID-læser. OnRfidRead kaldes af "hardwaren" (her: konsol-appen).
/// </summary>
public class RfidReader : IRfidReader
{
    public event EventHandler<RfidDetectedEventArgs>? RfidDetectedEvent;

    public void OnRfidRead(int id)
    {
        RfidDetectedEvent?.Invoke(this, new RfidDetectedEventArgs { Id = id });
    }
}
