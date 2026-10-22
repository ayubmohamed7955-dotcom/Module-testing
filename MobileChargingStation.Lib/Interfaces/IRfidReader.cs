namespace MobileChargingStation.Lib.Interfaces;

public class RfidDetectedEventArgs : EventArgs
{
    public int Id { get; set; }
}

public interface IRfidReader
{
    event EventHandler<RfidDetectedEventArgs>? RfidDetectedEvent;
}