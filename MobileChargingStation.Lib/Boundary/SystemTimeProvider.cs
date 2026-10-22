using System.Diagnostics.CodeAnalysis;
using MobileChargingStation.Lib.Interfaces;

namespace MobileChargingStation.Lib.Boundary;

/// <summary>
/// Rigtig tidskilde. Undtaget fra coverage: den returnerer blot systemuret,
/// som ikke kan styres i en unit test. Testbarheden opnås via ITimeProvider.
/// </summary>
[ExcludeFromCodeCoverage]
public class SystemTimeProvider : ITimeProvider
{
    public DateTime Now => DateTime.Now;
}
