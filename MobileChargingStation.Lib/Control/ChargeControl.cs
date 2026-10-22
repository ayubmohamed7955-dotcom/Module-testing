using MobileChargingStation.Lib.Boundary.Interfaces;
using MobileChargingStation.Lib.Interfaces;
 
namespace MobileChargingStation.Lib.Control;
 
public enum ChargeStatus
{
    NotCharging,   // 0 mA
    FullyCharged,  // 0 < I <= 5 mA
    Charging,      // 5 < I <= 500 mA
    Error          // I > 500 mA (eller negativ måling)
}
 
public class ChargeControl : IChargeControl
{
    public const double FullyChargedThreshold = 5.0;  // mA
    public const double OverloadThreshold = 500.0;    // mA
 
    public const string FullyChargedMessage = "Telefonen er fuldt opladet";
    public const string ChargingMessage = "Opladning i gang";
    public const string ErrorMessage = "Fejl under opladning - fjern telefonen";
 
    private readonly IUsbCharger _usbCharger;
    private readonly IDisplay _display;
 
    public ChargeStatus Status { get; private set; } = ChargeStatus.NotCharging;
 
    public ChargeControl(IUsbCharger usbCharger, IDisplay display)
    {
        _usbCharger = usbCharger;
        _display = display;
        _usbCharger.CurrentValueEvent += OnCurrentValueChanged;
    }
 
    public bool IsConnected() => _usbCharger.Connected;
 
    public void StartCharge() => _usbCharger.StartCharge();
 
    public void StopCharge() => _usbCharger.StopCharge();
 
    private void OnCurrentValueChanged(object? sender, CurrentEventArgs e)
    {
        var newStatus = Classify(e.Current);
 
        // Opdater kun når status ændrer sig, så displayet ikke spammes ved hvert event
        if (newStatus == Status) return;
        Status = newStatus;
 
        switch (newStatus)
        {
            case ChargeStatus.NotCharging:
                // Spec: displayet viser ikke noget om ladning.
                // Vi rører ikke displayet, så "fuldt opladet" bliver stående efter laderen er stoppet.
                break;
 
            case ChargeStatus.FullyCharged:
                _usbCharger.StopCharge();
                _display.ShowChargingMessage(FullyChargedMessage);
                break;
 
            case ChargeStatus.Charging:
                _display.ShowChargingMessage(ChargingMessage);
                break;
 
            case ChargeStatus.Error:
                _usbCharger.StopCharge();
                _display.ShowChargingMessage(ErrorMessage);
                break;
        }
    }
 
    private static ChargeStatus Classify(double current)
    {
        if (current < 0) return ChargeStatus.Error;  // Ikke i spec: negativ måling betragtes som fejl
        if (current == 0) return ChargeStatus.NotCharging;
        if (current <= FullyChargedThreshold) return ChargeStatus.FullyCharged;
        if (current <= OverloadThreshold) return ChargeStatus.Charging;
        return ChargeStatus.Error;
    }
}