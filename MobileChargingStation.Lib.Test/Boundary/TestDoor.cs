using MobileChargingStation.Lib.Boundary;

namespace MobileChargingStation.Lib.Test.Boundary;

[TestFixture]
public class TestDoor
{
    private Door _uut = null!;
    private int _openedCount;
    private int _closedCount;

    [SetUp]
    public void Setup()
    {
        _uut = new Door();
        _openedCount = 0;
        _closedCount = 0;
        _uut.DoorOpenedEvent += (_, _) => _openedCount++;
        _uut.DoorClosedEvent += (_, _) => _closedCount++;
    }

    // ---------- Starttilstand (Zero) ----------

    [Test]
    public void Ctor_DoorIsClosedAndUnlocked()
    {
        Assert.That(_uut.IsOpen, Is.False);
        Assert.That(_uut.IsLocked, Is.False);
    }

    // ---------- OnDoorOpen ----------

    [Test]
    public void OnDoorOpen_UnlockedAndClosed_RaisesDoorOpenedEventOnce()
    {
        _uut.OnDoorOpen();

        Assert.That(_openedCount, Is.EqualTo(1));
        Assert.That(_uut.IsOpen, Is.True);
    }

    [Test]
    public void OnDoorOpen_EventSenderIsDoor()
    {
        object? sender = null;
        _uut.DoorOpenedEvent += (s, _) => sender = s;

        _uut.OnDoorOpen();

        Assert.That(sender, Is.SameAs(_uut));
    }

    [Test]
    public void OnDoorOpen_Locked_NoEventAndStaysClosed()
    {
        _uut.LockDoor();

        _uut.OnDoorOpen();

        Assert.That(_openedCount, Is.Zero);
        Assert.That(_uut.IsOpen, Is.False);
    }

    [Test]
    public void OnDoorOpen_AlreadyOpen_NoSecondEvent()
    {
        _uut.OnDoorOpen();
        _uut.OnDoorOpen();

        Assert.That(_openedCount, Is.EqualTo(1));
    }

    [Test]
    public void OnDoorOpen_LockedThenUnlocked_RaisesEvent()
    {
        _uut.LockDoor();
        _uut.UnlockDoor();

        _uut.OnDoorOpen();

        Assert.That(_openedCount, Is.EqualTo(1));
    }

    [Test]
    public void OnDoorOpen_NoSubscribers_DoesNotThrow()
    {
        var door = new Door();

        Assert.DoesNotThrow(() => door.OnDoorOpen());
    }

    // ---------- OnDoorClose ----------

    [Test]
    public void OnDoorClose_Open_RaisesDoorClosedEventOnce()
    {
        _uut.OnDoorOpen();

        _uut.OnDoorClose();

        Assert.That(_closedCount, Is.EqualTo(1));
        Assert.That(_uut.IsOpen, Is.False);
    }

    [Test]
    public void OnDoorClose_EventSenderIsDoor()
    {
        object? sender = null;
        _uut.DoorClosedEvent += (s, _) => sender = s;
        _uut.OnDoorOpen();

        _uut.OnDoorClose();

        Assert.That(sender, Is.SameAs(_uut));
    }

    [Test]
    public void OnDoorClose_AlreadyClosed_NoEvent()
    {
        _uut.OnDoorClose();

        Assert.That(_closedCount, Is.Zero);
    }

    [Test]
    public void OnDoorClose_NoSubscribers_DoesNotThrow()
    {
        var door = new Door();
        door.OnDoorOpen();

        Assert.DoesNotThrow(() => door.OnDoorClose());
    }

    [Test]
    public void OpenCloseOpenClose_RaisesTwoOfEach()
    {
        _uut.OnDoorOpen();
        _uut.OnDoorClose();
        _uut.OnDoorOpen();
        _uut.OnDoorClose();

        Assert.That(_openedCount, Is.EqualTo(2));
        Assert.That(_closedCount, Is.EqualTo(2));
    }

    // ---------- LockDoor / UnlockDoor ----------

    [Test]
    public void LockDoor_SetsIsLocked()
    {
        _uut.LockDoor();

        Assert.That(_uut.IsLocked, Is.True);
    }

    [Test]
    public void UnlockDoor_AfterLock_ClearsIsLocked()
    {
        _uut.LockDoor();

        _uut.UnlockDoor();

        Assert.That(_uut.IsLocked, Is.False);
    }

    [Test]
    public void LockAndUnlock_DoNotRaiseEvents()
    {
        _uut.LockDoor();
        _uut.UnlockDoor();

        Assert.That(_openedCount, Is.Zero);
        Assert.That(_closedCount, Is.Zero);
    }
}
