using MobileChargingStation.Lib.Interfaces;

namespace MobileChargingStation.Lib.Control;

// Tilstande svarende til tilstandsdiagrammet (figur 4)
public enum StationState
{
    Available,
    DoorOpen,
    Locked
}

public class StationControl
{
    // Beskeder til displayets brugerområde (figur 3 og 4)
    public const string ReadRfidMessage = "Indlæs RFID";
    public const string ConnectPhoneMessage = "Tilslut telefon";
    public const string ConnectionErrorMessage = "Tilslutningsfejl";
    public const string StationOccupiedMessage = "Ladeskab optaget";
    public const string RfidErrorMessage = "RFID fejl";
    public const string RemovePhoneMessage = "Fjern telefon";

    private readonly IDoor _door;
    private readonly IRfidReader _rfidReader;
    private readonly IChargeControl _charger;
    private readonly IDisplay _display;
    private readonly ILogFile _logFile;

    private int _oldId;

    public StationState State { get; private set; }

    public StationControl(
        IDoor door,
        IRfidReader rfidReader,
        IChargeControl charger,
        IDisplay display,
        ILogFile logFile)
    {
        _door = door;
        _rfidReader = rfidReader;
        _charger = charger;
        _display = display;
        _logFile = logFile;

        _door.DoorOpenedEvent += OnDoorOpened;
        _door.DoorClosedEvent += OnDoorClosed;
        _rfidReader.RfidDetectedEvent += OnRfidDetected;

        // Initial transition: / door.UnlockDoor(), Show "Indlæs RFID"
        _door.UnlockDoor();
        _display.ShowStationMessage(ReadRfidMessage);
        State = StationState.Available;
    }

    private void OnDoorOpened(object? sender, EventArgs e)
    {
        switch (State)
        {
            case StationState.Available:
                _display.ShowStationMessage(ConnectPhoneMessage);
                State = StationState.DoorOpen;
                break;

            case StationState.DoorOpen:
            case StationState.Locked:
                // Ikke defineret i diagrammet: ignoreres
                break;
        }
    }

    private void OnDoorClosed(object? sender, EventArgs e)
    {
        switch (State)
        {
            case StationState.DoorOpen:
                _display.ShowStationMessage(ReadRfidMessage);
                State = StationState.Available;
                break;

            case StationState.Available:
            case StationState.Locked:
                // Ikke defineret i diagrammet: ignoreres
                break;
        }
    }

    private void OnRfidDetected(object? sender, RfidDetectedEventArgs e)
    {
        int id = e.Id;

        switch (State)
        {
            case StationState.Available:
                if (_charger.IsConnected())
                {
                    _oldId = id;
                    _door.LockDoor();
                    _charger.StartCharge();
                    _logFile.LogDoorLocked(id.ToString());
                    _display.ShowStationMessage(StationOccupiedMessage);
                    State = StationState.Locked;
                }
                else
                {
                    _display.ShowStationMessage(ConnectionErrorMessage);
                }
                break;

            case StationState.DoorOpen:
                // Lågen er åben: RFID ignoreres
                break;

            case StationState.Locked:
                if (id == _oldId)
                {
                    _charger.StopCharge();
                    _door.UnlockDoor();
                    _logFile.LogDoorUnlocked(id.ToString());
                    _display.ShowStationMessage(RemovePhoneMessage);
                    State = StationState.Available;
                }
                else
                {
                    _display.ShowStationMessage(RfidErrorMessage);
                }
                break;
        }
    }
}
