using MobileChargingStation.Lib.Control;
using MobileChargingStation.Lib.Interfaces;
using NSubstitute;

namespace MobileChargingStation.Lib.Test.Control;

[TestFixture]
public class StationControlTest
{
    private IDoor _door = null!;
    private IRfidReader _rfidReader = null!;
    private IChargeControl _charger = null!;
    private IDisplay _display = null!;
    private ILogFile _logFile = null!;
    private StationControl _uut = null!;

    [SetUp]
    public void Setup()
    {
        _door = Substitute.For<IDoor>();
        _rfidReader = Substitute.For<IRfidReader>();
        _charger = Substitute.For<IChargeControl>();
        _display = Substitute.For<IDisplay>();
        _logFile = Substitute.For<ILogFile>();

        _uut = new StationControl(_door, _rfidReader, _charger, _display, _logFile);
    }

    // ---------- Hjælpemetoder ----------

    private void OpenDoor() => _door.DoorOpenedEvent += Raise.Event();
    private void CloseDoor() => _door.DoorClosedEvent += Raise.Event();

    private void ReadRfid(int id) =>
        _rfidReader.RfidDetectedEvent += Raise.EventWith(new RfidDetectedEventArgs { Id = id });

    // Bringer stationen i Locked med det givne id og nulstiller modtagne kald
    private void LockWith(int id)
    {
        _charger.IsConnected().Returns(true);
        ReadRfid(id);
        ClearAllReceivedCalls();
    }

    private void ClearAllReceivedCalls()
    {
        _door.ClearReceivedCalls();
        _charger.ClearReceivedCalls();
        _display.ClearReceivedCalls();
        _logFile.ClearReceivedCalls();
    }

    // ---------- Initial transition ----------

    [Test]
    public void Ctor_StateIsAvailable()
    {
        Assert.That(_uut.State, Is.EqualTo(StationState.Available));
    }

    [Test]
    public void Ctor_UnlocksDoor()
    {
        _door.Received(1).UnlockDoor();
    }

    [Test]
    public void Ctor_ShowsReadRfid()
    {
        _display.Received(1).ShowStationMessage(StationControl.ReadRfidMessage);
    }

    // ---------- Available -> DoorOpen -> Available ----------

    [Test]
    public void Available_DoorOpened_StateIsDoorOpen()
    {
        OpenDoor();
        Assert.That(_uut.State, Is.EqualTo(StationState.DoorOpen));
    }

    [Test]
    public void Available_DoorOpened_ShowsConnectPhone()
    {
        OpenDoor();
        _display.Received(1).ShowStationMessage(StationControl.ConnectPhoneMessage);
    }

    [Test]
    public void DoorOpen_DoorClosed_StateIsAvailable()
    {
        OpenDoor();
        CloseDoor();
        Assert.That(_uut.State, Is.EqualTo(StationState.Available));
    }

    [Test]
    public void DoorOpen_DoorClosed_ShowsReadRfid()
    {
        OpenDoor();
        _display.ClearReceivedCalls();

        CloseDoor();

        _display.Received(1).ShowStationMessage(StationControl.ReadRfidMessage);
    }

    // ---------- Available: RFID uden tilsluttet telefon ----------

    [Test]
    public void Available_RfidNotConnected_ShowsConnectionError()
    {
        _charger.IsConnected().Returns(false);
        ReadRfid(12);
        _display.Received(1).ShowStationMessage(StationControl.ConnectionErrorMessage);
    }

    [Test]
    public void Available_RfidNotConnected_StaysAvailable_NothingLockedOrLogged()
    {
        _charger.IsConnected().Returns(false);
        _door.ClearReceivedCalls();

        ReadRfid(12);

        Assert.That(_uut.State, Is.EqualTo(StationState.Available));
        _door.DidNotReceive().LockDoor();
        _charger.DidNotReceive().StartCharge();
        _logFile.DidNotReceiveWithAnyArgs().LogDoorLocked(default!);
    }

    // ---------- Available: RFID med tilsluttet telefon -> Locked ----------

    [Test]
    public void Available_RfidConnected_StateIsLocked()
    {
        _charger.IsConnected().Returns(true);
        ReadRfid(12);
        Assert.That(_uut.State, Is.EqualTo(StationState.Locked));
    }

    [Test]
    public void Available_RfidConnected_LocksDoor_StartsCharge_Logs_ShowsOccupied()
    {
        _charger.IsConnected().Returns(true);

        ReadRfid(12);

        _door.Received(1).LockDoor();
        _charger.Received(1).StartCharge();
        _logFile.Received(1).LogDoorLocked("12");
        _display.Received(1).ShowStationMessage(StationControl.StationOccupiedMessage);
    }

    // BVA/EP på id-værdien: id'et videregives uændret til loggen
    [TestCase(int.MinValue)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(int.MaxValue)]
    public void Available_RfidConnected_LogsCorrectId(int id)
    {
        _charger.IsConnected().Returns(true);
        ReadRfid(id);
        _logFile.Received(1).LogDoorLocked(id.ToString());
    }

    // ---------- Locked: korrekt RFID -> Available ----------

    [Test]
    public void Locked_SameRfid_StateIsAvailable()
    {
        LockWith(12);
        ReadRfid(12);
        Assert.That(_uut.State, Is.EqualTo(StationState.Available));
    }

    [Test]
    public void Locked_SameRfid_StopsCharge_UnlocksDoor_Logs_ShowsRemovePhone()
    {
        LockWith(12);

        ReadRfid(12);

        Received.InOrder(() =>
        {
            _charger.StopCharge();
            _door.UnlockDoor();
            _logFile.LogDoorUnlocked("12");
            _display.ShowStationMessage(StationControl.RemovePhoneMessage);
        });
    }

    // ---------- Locked: forkert RFID ----------

    [TestCase(11)] // lige under
    [TestCase(13)] // lige over
    [TestCase(0)]
    [TestCase(-12)]
    public void Locked_DifferentRfid_ShowsRfidError_StaysLocked(int wrongId)
    {
        LockWith(12);

        ReadRfid(wrongId);

        _display.Received(1).ShowStationMessage(StationControl.RfidErrorMessage);
        Assert.That(_uut.State, Is.EqualTo(StationState.Locked));
        _door.DidNotReceive().UnlockDoor();
        _charger.DidNotReceive().StopCharge();
        _logFile.DidNotReceiveWithAnyArgs().LogDoorUnlocked(default!);
    }

    [Test]
    public void Locked_WrongThenCorrectRfid_Unlocks()
    {
        LockWith(12);

        ReadRfid(99);
        ReadRfid(12);

        _door.Received(1).UnlockDoor();
        Assert.That(_uut.State, Is.EqualTo(StationState.Available));
    }

    // ---------- Events der ikke står i diagrammet: ignoreres ----------

    [Test]
    public void DoorOpen_Rfid_IsIgnored()
    {
        OpenDoor();
        ClearAllReceivedCalls();

        ReadRfid(12);

        Assert.That(_uut.State, Is.EqualTo(StationState.DoorOpen));
        _charger.DidNotReceive().IsConnected();
        _door.DidNotReceive().LockDoor();
        _display.DidNotReceiveWithAnyArgs().ShowStationMessage(default!);
    }

    [Test]
    public void DoorOpen_DoorOpenedAgain_IsIgnored()
    {
        OpenDoor();
        _display.ClearReceivedCalls();

        OpenDoor();

        Assert.That(_uut.State, Is.EqualTo(StationState.DoorOpen));
        _display.DidNotReceiveWithAnyArgs().ShowStationMessage(default!);
    }

    [Test]
    public void Available_DoorClosed_IsIgnored()
    {
        _display.ClearReceivedCalls();

        CloseDoor();

        Assert.That(_uut.State, Is.EqualTo(StationState.Available));
        _display.DidNotReceiveWithAnyArgs().ShowStationMessage(default!);
    }

    [Test]
    public void Locked_DoorOpened_IsIgnored()
    {
        LockWith(12);

        OpenDoor();

        Assert.That(_uut.State, Is.EqualTo(StationState.Locked));
        _display.DidNotReceiveWithAnyArgs().ShowStationMessage(default!);
    }

    [Test]
    public void Locked_DoorClosed_IsIgnored()
    {
        LockWith(12);

        CloseDoor();

        Assert.That(_uut.State, Is.EqualTo(StationState.Locked));
        _display.DidNotReceiveWithAnyArgs().ShowStationMessage(default!);
    }

    // ---------- Hele forløbet (figur 3) ----------

    [Test]
    public void FullUseCase_CorrectSequenceOfMessages()
    {
        _charger.IsConnected().Returns(true);

        OpenDoor();
        CloseDoor();
        ReadRfid(42);
        ReadRfid(42);
        OpenDoor();
        CloseDoor();

        Received.InOrder(() =>
        {
            _display.ShowStationMessage(StationControl.ReadRfidMessage);
            _display.ShowStationMessage(StationControl.ConnectPhoneMessage);
            _display.ShowStationMessage(StationControl.ReadRfidMessage);
            _display.ShowStationMessage(StationControl.StationOccupiedMessage);
            _display.ShowStationMessage(StationControl.RemovePhoneMessage);
            _display.ShowStationMessage(StationControl.ConnectPhoneMessage);
            _display.ShowStationMessage(StationControl.ReadRfidMessage);
        });
        Assert.That(_uut.State, Is.EqualTo(StationState.Available));
    }

    [Test]
    public void SecondUser_CanLockWithNewId_AfterFirstUnlocked()
    {
        LockWith(1);
        ReadRfid(1);   // bruger 1 låser op

        ReadRfid(2);   // bruger 2 låser (IsConnected er stadig true)
        ReadRfid(1);   // gammelt id virker ikke længere

        _display.Received(1).ShowStationMessage(StationControl.RfidErrorMessage);
        Assert.That(_uut.State, Is.EqualTo(StationState.Locked));
    }
}
