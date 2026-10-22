using System.Diagnostics.CodeAnalysis;
using MobileChargingStation.Lib.Interfaces;

namespace MobileChargingStation.Lib.Boundary;

/// <summary>
/// Rigtig filskriver. Undtaget fra coverage: den rører filsystemet og
/// er en tynd wrapper om File.AppendAllText. Testbarheden opnås via IFileWriter.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileWriter : IFileWriter
{
    private readonly string _path;

    public FileWriter(string path = "logfile.txt")
    {
        _path = path;
    }

    public void AppendLine(string line)
    {
        File.AppendAllText(_path, line + Environment.NewLine);
    }
}
