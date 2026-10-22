using MobileChargingStation.Lib.Boundary;
using MobileChargingStation.Lib.Control;

namespace Mobilechargingstation.ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        // ---------- Samlet system ----------
        // Boundary-klasser (simuleret hardware)
        var door = new Door();
        var rfidReader = new RfidReader();
        var display = new Display();
        var usbCharger = new UsbChargerSimulator();
        var logFile = new LogFile(new SystemTimeProvider(), new FileWriter("ladeskab.log"));

        // Control-klasser
        var chargeControl = new ChargeControl(usbCharger, display);
        var stationControl = new StationControl(door, rfidReader, chargeControl, display, logFile);

        PrintMenu();

        // ---------- Simpelt forløb styret fra konsollen ----------
        bool finish = false;
        do
        {
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                continue;

            switch (char.ToUpper(input[0]))
            {
                case 'E':
                    finish = true;
                    break;

                case 'O':
                    door.OnDoorOpen();
                    break;

                case 'C':
                    door.OnDoorClose();
                    break;

                case 'T':
                    usbCharger.SimulateConnected(true);
                    Console.WriteLine("(Telefon tilsluttet)");
                    break;

                case 'F':
                    usbCharger.SimulateConnected(false);
                    Console.WriteLine("(Telefon frakoblet)");
                    break;

                case 'K':
                    usbCharger.SimulateOverload(true);
                    Console.WriteLine("(Kortslutning simuleret)");
                    break;

                case 'R':
                    Console.WriteLine("Indtast RFID id: ");
                    string? idString = Console.ReadLine();

                    if (int.TryParse(idString, out int id))
                        rfidReader.OnRfidRead(id);
                    else
                        Console.WriteLine("Ugyldigt RFID id - skal være et heltal");
                    break;

                default:
                    PrintMenu();
                    break;
            }
        } while (!finish);
    }

    private static void PrintMenu()
    {
        Console.WriteLine("""
            ===== Ladeskab =====
            O: Åbn låge
            C: Luk låge
            T: Tilslut telefon
            F: Frakobl telefon
            K: Simulér kortslutning
            R: Indlæs RFID
            E: Afslut
            """);
    }
}