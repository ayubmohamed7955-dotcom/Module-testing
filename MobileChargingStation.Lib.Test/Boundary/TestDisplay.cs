using MobileChargingStation.Lib.Boundary;

namespace MobileChargingStation.Lib.Test.Boundary;

[TestFixture]
public class TestDisplay
{
    private Display _uut = null!;
    private StringWriter _output = null!;
    private TextWriter _originalOut = null!;

    [SetUp]
    public void Setup()
    {
        _originalOut = Console.Out;
        _output = new StringWriter();
        Console.SetOut(_output);
        _uut = new Display();
    }

    [TearDown]
    public void TearDown()
    {
        Console.SetOut(_originalOut);
        _output.Dispose();
    }

    [Test]
    public void ShowStationMessage_WritesPrefixAndMessageAsLine()
    {
        _uut.ShowStationMessage("Tilslut telefon");

        Assert.That(_output.ToString(),
            Is.EqualTo(Display.StationPrefix + "Tilslut telefon" + Environment.NewLine));
    }

    [Test]
    public void ShowChargingMessage_WritesPrefixAndMessageAsLine()
    {
        _uut.ShowChargingMessage("Opladning i gang");

        Assert.That(_output.ToString(),
            Is.EqualTo(Display.ChargingPrefix + "Opladning i gang" + Environment.NewLine));
    }

    [Test]
    public void ShowStationMessage_DoesNotUseChargingArea()
    {
        _uut.ShowStationMessage("Hej");

        Assert.That(_output.ToString(), Does.Not.Contain(Display.ChargingPrefix));
    }

    [Test]
    public void ShowChargingMessage_DoesNotUseStationArea()
    {
        _uut.ShowChargingMessage("Hej");

        Assert.That(_output.ToString(), Does.Not.Contain(Display.StationPrefix));
    }

    [Test]
    public void ShowStationMessage_EmptyMessage_WritesOnlyPrefix()
    {
        _uut.ShowStationMessage("");

        Assert.That(_output.ToString(),
            Is.EqualTo(Display.StationPrefix + Environment.NewLine));
    }

    [Test]
    public void BothAreas_WrittenInCallOrder()
    {
        _uut.ShowStationMessage("A");
        _uut.ShowChargingMessage("B");

        var expected = Display.StationPrefix + "A" + Environment.NewLine
                     + Display.ChargingPrefix + "B" + Environment.NewLine;
        Assert.That(_output.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void Prefixes_AreDifferent()
    {
        Assert.That(Display.StationPrefix, Is.Not.EqualTo(Display.ChargingPrefix));
    }
}
