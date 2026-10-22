using MobileChargingStation.Lib.Boundary;
using MobileChargingStation.Lib.Control;

namespace Mobilechargingstation.ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        // Assemble your system here from all the classes

        bool finish = false;
        do
        {
            string input;
            System.Console.WriteLine("Indtast E, O, C, R: ");
            input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                continue;

            switch (input[0])
            {
                case 'E':
                    finish = true;
                    break;

                case 'O':
                    // TODO: aktiveres når Door er implementeret
                    // door.OnDoorOpen();
                    break;

                case 'C':
                    // TODO: aktiveres når Door er implementeret
                    // door.OnDoorClose();
                    break;

                case 'R':
                    System.Console.WriteLine("Indtast RFID id: ");
                    string idString = System.Console.ReadLine();

                    int id = Convert.ToInt32(idString);
                    // TODO: aktiveres når RFIDReader er implementeret
                    // rfidReader.OnRfidRead(id);
                    break;

                default:
                    break;
            }
        } while (!finish);
    }
}
