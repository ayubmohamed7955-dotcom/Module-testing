using System.Globalization;
using MobileChargingStation.Lib.Interfaces;

namespace MobileChargingStation.Lib.Boundary;

/// <summary>
/// Logger låsning/oplåsning med tidsstempel (HH:mm:ss) og RFID.
/// Tid og fil er indkapslet bag ITimeProvider og IFileWriter, så klassen kan testes med fakes.
/// </summary>
public class LogFile : ILogFile
{
    public const string TimeFormat = "HH:mm:ss";
    public const string LockedText = "Skab låst med RFID";
    public const string UnlockedText = "Skab låst op med RFID";

    private readonly ITimeProvider _timeProvider;
    private readonly IFileWriter _fileWriter;

    public LogFile(ITimeProvider timeProvider, IFileWriter fileWriter)
    {
        _timeProvider = timeProvider;
        _fileWriter = fileWriter;
    }

    public void LogDoorLocked(string id) => Write(LockedText, id);

    public void LogDoorUnlocked(string id) => Write(UnlockedText, id);

    private void Write(string text, string id)
    {
        // InvariantCulture: ellers bliver ':' til '.' på en dansk maskine (da-DK)
        var timestamp = _timeProvider.Now.ToString(TimeFormat, CultureInfo.InvariantCulture);
        _fileWriter.AppendLine($"{timestamp}: {text}: {id}");
    }
}
