namespace MobileChargingStation.Lib.Interfaces;

public interface IDisplay
{
    void ShowStationMessage(string message);   // område 1: brugerinstruktioner
    void ShowChargingMessage(string message);  // område 2: ladestatus
}