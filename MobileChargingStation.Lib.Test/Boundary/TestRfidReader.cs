using MobileChargingStation.Lib.Boundary;
using MobileChargingStation.Lib.Interfaces;

namespace MobileChargingStation.Lib.Test.Boundary;

[TestFixture]
public class TestRfidReader
{
    private RfidReader _uut = null!;
    private RfidDetectedEventArgs? _receivedArgs;
    private object? _receivedSender;
    private int _eventCount;

    [SetUp]
    public void Setup()
    {
        _uut = new RfidReader();
        _receivedArgs = null;
        _receivedSender = null;
        _eventCount = 0;
        _uut.RfidDetectedEvent += (sender, args) =>
        {
            _receivedSender = sender;
            _receivedArgs = args;
            _eventCount++;
        };
    }

    [Test]
    public void Ctor_NoEventRaised()
    {
        Assert.That(_eventCount, Is.Zero);
    }

    [Test]
    public void OnRfidRead_RaisesEventOnce()
    {
        _uut.OnRfidRead(42);

        Assert.That(_eventCount, Is.EqualTo(1));
    }

    [Test]
    public void OnRfidRead_SenderIsReader()
    {
        _uut.OnRfidRead(42);

        Assert.That(_receivedSender, Is.SameAs(_uut));
    }

    // EP/BVA: nul, positive, negative og ekstremværdier for int
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(-1)]
    [TestCase(1234)]
    [TestCase(int.MaxValue)]
    [TestCase(int.MinValue)]
    public void OnRfidRead_EventCarriesId(int id)
    {
        _uut.OnRfidRead(id);

        Assert.That(_receivedArgs!.Id, Is.EqualTo(id));
    }

    [Test]
    public void OnRfidRead_Twice_RaisesTwoEventsWithLatestId()
    {
        _uut.OnRfidRead(1);
        _uut.OnRfidRead(2);

        Assert.That(_eventCount, Is.EqualTo(2));
        Assert.That(_receivedArgs!.Id, Is.EqualTo(2));
    }

    [Test]
    public void OnRfidRead_NoSubscribers_DoesNotThrow()
    {
        var reader = new RfidReader();

        Assert.DoesNotThrow(() => reader.OnRfidRead(1));
    }
}
