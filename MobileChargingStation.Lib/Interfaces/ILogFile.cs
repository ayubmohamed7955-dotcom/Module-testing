namespace MobileChargingStation.Lib.Interfaces;

public interface ILogFile
{
    void LogDoorLocked(string id);
    void LogDoorUnlocked(string id);
}