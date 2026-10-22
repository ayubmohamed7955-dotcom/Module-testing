using MobileChargingStation.Lib.Boundary;
using MobileChargingStation.Lib.Interfaces;
using NSubstitute;

namespace MobileChargingStation.Lib.Test.Boundary;

[TestFixture]
public class TestLogFile
{
    private ITimeProvider _timeProvider = null!;
    private IFileWriter _fileWriter = null!;
    private LogFile _uut = null!;

    [SetUp]
    public void Setup()
    {
        _timeProvider = Substitute.For<ITimeProvider>();
        _fileWriter = Substitute.For<IFileWriter>();
        _timeProvider.Now.Returns(new DateTime(2026, 10, 22, 14, 5, 9));
        _uut = new LogFile(_timeProvider, _fileWriter);
    }

    [Test]
    public void Ctor_WritesNothing()
    {
        _fileWriter.DidNotReceiveWithAnyArgs().AppendLine(default!);
    }

    [Test]
    public void LogDoorLocked_WritesTimestampTextAndId()
    {
        _uut.LogDoorLocked("1234");

        _fileWriter.Received(1).AppendLine("14:05:09: Skab låst med RFID: 1234");
    }

    [Test]
    public void LogDoorUnlocked_WritesTimestampTextAndId()
    {
        _uut.LogDoorUnlocked("1234");

        _fileWriter.Received(1).AppendLine("14:05:09: Skab låst op med RFID: 1234");
    }

    [Test]
    public void LogDoorLocked_DoesNotWriteUnlockedText()
    {
        _uut.LogDoorLocked("1");

        _fileWriter.DidNotReceive().AppendLine(Arg.Is<string>(s => s.Contains(LogFile.UnlockedText)));
    }

    // BVA på tidsstemplet: midnat, sidste sekund og 24-timers format (ikke AM/PM)
    [TestCase(0, 0, 0, "00:00:00")]
    [TestCase(23, 59, 59, "23:59:59")]
    [TestCase(13, 0, 0, "13:00:00")]
    [TestCase(9, 8, 7, "09:08:07")]
    public void LogDoorLocked_TimestampFormattedAsHHmmss(int h, int m, int s, string expected)
    {
        _timeProvider.Now.Returns(new DateTime(2026, 1, 1, h, m, s));

        _uut.LogDoorLocked("1");

        _fileWriter.Received(1).AppendLine(Arg.Is<string>(line => line.StartsWith(expected + ": ")));
    }

    [Test]
    public void LogDoorLocked_TimestampHasNoDateOrMilliseconds()
    {
        _timeProvider.Now.Returns(new DateTime(2026, 10, 22, 14, 5, 9, 999));

        _uut.LogDoorLocked("1");

        _fileWriter.Received(1).AppendLine("14:05:09: Skab låst med RFID: 1");
    }

    // EP på RFID: nul, negativ, ekstremværdi og tom streng (StationControl sender int.ToString())
    [TestCase("0")]
    [TestCase("-1")]
    [TestCase("2147483647")]
    [TestCase("")]
    public void LogDoorUnlocked_IdWrittenAtEnd(string id)
    {
        _uut.LogDoorUnlocked(id);

        _fileWriter.Received(1).AppendLine(Arg.Is<string>(line => line.EndsWith(": " + id)));
    }

    [Test]
    [SetCulture("da-DK")]
    public void LogDoorLocked_DanishCulture_StillUsesColonSeparator()
    {
        _uut.LogDoorLocked("1");

        _fileWriter.Received(1).AppendLine("14:05:09: Skab låst med RFID: 1");
    }

    [Test]
    public void LogLockedThenUnlocked_WritesTwoLinesInOrder()
    {
        _uut.LogDoorLocked("7");
        _uut.LogDoorUnlocked("7");

        Received.InOrder(() =>
        {
            _fileWriter.AppendLine("14:05:09: Skab låst med RFID: 7");
            _fileWriter.AppendLine("14:05:09: Skab låst op med RFID: 7");
        });
    }

    [Test]
    public void LogDoorLocked_ReadsTimeAtCallTime()
    {
        _timeProvider.Now.Returns(new DateTime(2026, 1, 1, 10, 0, 0),
                                  new DateTime(2026, 1, 1, 11, 0, 0));

        _uut.LogDoorLocked("1");
        _uut.LogDoorUnlocked("1");

        _fileWriter.Received(1).AppendLine("10:00:00: Skab låst med RFID: 1");
        _fileWriter.Received(1).AppendLine("11:00:00: Skab låst op med RFID: 1");
    }
}
