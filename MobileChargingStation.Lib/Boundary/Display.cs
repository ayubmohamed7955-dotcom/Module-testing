using MobileChargingStation.Lib.Interfaces;

namespace MobileChargingStation.Lib.Boundary;

/// <summary>
/// Display med to områder: brugerinstruktioner (station) og ladestatus (charging).
/// Simuleres ved at skrive til konsollen.
/// </summary>
public class Display : IDisplay
{
    public const string StationPrefix = "[Station] ";
    public const string ChargingPrefix = "[Ladning] ";

    public void ShowStationMessage(string message)
    {
        Console.WriteLine(StationPrefix + message);
    }

    public void ShowChargingMessage(string message)
    {
        Console.WriteLine(ChargingPrefix + message);
    }
}
