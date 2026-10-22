using MobileChargingStation.Lib.Control;
using MobileChargingStation.Lib.Interfaces;
using NSubstitute;
using NUnit.Framework;
using MobileChargingStation.Lib.Boundary.Interfaces;

namespace MobileChargingStation.Lib.Test;

[TestFixture]
public class TestChargeControl
{
    private IUsbCharger _usbCharger = null!;
    private IDisplay _display = null!;
    private ChargeControl _uut = null!;

    [SetUp]
    public void Setup()
    {
        _usbCharger = Substitute.For<IUsbCharger>();
        _display = Substitute.For<IDisplay>();
        _uut = new ChargeControl(_usbCharger, _display);
    }

    private void RaiseCurrent(double current) =>
        _usbCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs { Current = current });

    // ---------- Videregivelse til IUsbCharger ----------

    [TestCase(true)]
    [TestCase(false)]
    public void IsConnected_ReturnsUsbChargerConnected(bool connected)
    {
        _usbCharger.Connected.Returns(connected);
        Assert.That(_uut.IsConnected(), Is.EqualTo(connected));
    }

    [Test]
    public void StartCharge_CallsUsbChargerStartCharge()
    {
        _uut.StartCharge();
        _usbCharger.Received(1).StartCharge();
    }

    [Test]
    public void StopCharge_CallsUsbChargerStopCharge()
    {
        _uut.StopCharge();
        _usbCharger.Received(1).StopCharge();
    }

    // ---------- Startværdi ----------

    [Test]
    public void Status_Initially_IsNotCharging()
    {
        Assert.That(_uut.Status, Is.EqualTo(ChargeStatus.NotCharging));
    }

    // ---------- BVA: 0 mA (ingen forbindelse / ikke startet) ----------

    [Test]
    public void CurrentZero_NoDisplayUpdate_NoStop()
    {
        RaiseCurrent(0);

        _display.DidNotReceiveWithAnyArgs().ShowChargingMessage(default!);
        _usbCharger.DidNotReceive().StopCharge();
        Assert.That(_uut.Status, Is.EqualTo(ChargeStatus.NotCharging));
    }

    // ---------- BVA: 0 < I <= 5 mA (fuldt opladet) ----------

    [TestCase(0.001)]   // lige over nedre grænse
    [TestCase(2.5)]     // midt i intervallet (EP)
    [TestCase(5.0)]     // præcis på øvre grænse
    public void CurrentFullyCharged_StopsCharge_ShowsFullyCharged(double current)
    {
        RaiseCurrent(current);

        _usbCharger.Received(1).StopCharge();
        _display.Received(1).ShowChargingMessage(ChargeControl.FullyChargedMessage);
        Assert.That(_uut.Status, Is.EqualTo(ChargeStatus.FullyCharged));
    }

    // ---------- BVA: 5 < I <= 500 mA (normal opladning) ----------

    [TestCase(5.001)]   // lige over nedre grænse
    [TestCase(250)]     // midt i intervallet (EP)
    [TestCase(500.0)]   // præcis på øvre grænse
    public void CurrentCharging_ShowsCharging_DoesNotStop(double current)
    {
        RaiseCurrent(current);

        _display.Received(1).ShowChargingMessage(ChargeControl.ChargingMessage);
        _usbCharger.DidNotReceive().StopCharge();
        Assert.That(_uut.Status, Is.EqualTo(ChargeStatus.Charging));
    }

    // ---------- BVA: I > 500 mA (fejl / kortslutning) ----------

    [TestCase(500.001)] // lige over grænsen
    [TestCase(10000)]   // langt over (EP)
    public void CurrentOverload_StopsCharge_ShowsError(double current)
    {
        RaiseCurrent(current);

        _usbCharger.Received(1).StopCharge();
        _display.Received(1).ShowChargingMessage(ChargeControl.ErrorMessage);
        Assert.That(_uut.Status, Is.EqualTo(ChargeStatus.Error));
    }

    // ---------- Uden for spec: negativ måling ----------

    [TestCase(-0.001)]
    [TestCase(-100)]
    public void CurrentNegative_TreatedAsError(double current)
    {
        RaiseCurrent(current);

        _usbCharger.Received(1).StopCharge();
        _display.Received(1).ShowChargingMessage(ChargeControl.ErrorMessage);
    }

    // ---------- ZOMBIE: Many - gentagne events med samme status ----------

    [Test]
    public void SameStatusRepeated_DisplayUpdatedOnlyOnce()
    {
        RaiseCurrent(300);
        RaiseCurrent(400);
        RaiseCurrent(450);

        _display.Received(1).ShowChargingMessage(ChargeControl.ChargingMessage);
    }

    // ---------- Sekvenser af statusskift ----------

    [Test]
    public void ChargingThenFullyCharged_ShowsBothMessagesInOrder()
    {
        RaiseCurrent(300);
        RaiseCurrent(3);

        Received.InOrder(() =>
        {
            _display.ShowChargingMessage(ChargeControl.ChargingMessage);
            _display.ShowChargingMessage(ChargeControl.FullyChargedMessage);
        });
        _usbCharger.Received(1).StopCharge();
    }

    [Test]
    public void FullyChargedThenZero_KeepsFullyChargedMessage()
    {
        RaiseCurrent(3);
        _display.ClearReceivedCalls();

        RaiseCurrent(0);

        _display.DidNotReceiveWithAnyArgs().ShowChargingMessage(default!);
        Assert.That(_uut.Status, Is.EqualTo(ChargeStatus.NotCharging));
    }

    [Test]
    public void ErrorThenCharging_ShowsChargingAgain()
    {
        RaiseCurrent(600);
        RaiseCurrent(300);

        _display.Received(1).ShowChargingMessage(ChargeControl.ErrorMessage);
        _display.Received(1).ShowChargingMessage(ChargeControl.ChargingMessage);
    }
}