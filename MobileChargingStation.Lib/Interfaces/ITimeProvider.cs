namespace MobileChargingStation.Lib.Interfaces;

public interface ITimeProvider
{
    DateTime Now { get; }
}