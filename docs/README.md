# Dokumentation – Ladeskab

[← Tilbage til forsiden](../README.md)

## Indhold
1. [Overblik over designet](#1-overblik-over-designet)
2. [StationControl](#2-stationcontrol)
3. [ChargeControl og opladning](#3-chargecontrol-og-opladning)
4. [Boundary-klasser: Door, RfidReader, Display og LogFile](#4-boundary-klasser-door-rfidreader-display-og-logfile)
5. [Teststrategi](#5-teststrategi)
6. [Coverage](#6-coverage)
7. [Continuous Integration](#7-continuous-integration)
8. [Refleksion over designet](#8-refleksion-over-designet)
9. [Arbejdsfordeling](#9-arbejdsfordeling)
10. [Refleksion over fælles repository og CI](#10-refleksion-over-fælles-repository-og-ci)

Billeder ligger i `docs/images/`.

---

## 1. Overblik over designet
*(Person 2: klassediagram + kort beskrivelse af lagdelingen control/boundary.)*

## 2. StationControl
Se [StationControl.md](StationControl.md) for tilstandsdiagram, sekvensdiagram, transitioner og designrefleksion.

## 3. ChargeControl og opladning
*(Mohamad: sekvensdiagram for opladning og beskrivelse af strømintervallerne.)*

## 4. Boundary-klasser: Door, RfidReader, Display og LogFile

Boundary-klasserne er systemets grænseflade mod hardware og omverden (låge, RFID-læser, display og logfil). Hver af dem implementerer et interface fra `MobileChargingStation.Lib.Interfaces`, så `StationControl` og `ChargeControl` kun kender interfacet og kan testes med fakes (NSubstitute) i stedet for den rigtige klasse.

### Door (`IDoor`)
Simulerer lågen. `OnDoorOpen()` og `OnDoorClose()` svarer til, at hardwaren registrerer at lågen åbnes eller lukkes, og omsættes til eventsene `DoorOpenedEvent` og `DoorClosedEvent` (Observer Pattern med C#-events). `StationControl` styrer låsen med `LockDoor()` og `UnlockDoor()`.

Door håndhæver selv to fysiske regler:
- En **låst** låge kan ikke åbnes – `OnDoorOpen()` ignoreres, og der fyres intet event.
- Et event fyres kun ved en reel tilstandsændring (åbn en åben låge eller luk en lukket låge giver intet event).

Dermed kan `StationControl` stole på, at den ikke modtager et "dør åbnet"-event, mens skabet er låst.

### RfidReader (`IRfidReader`)
Simulerer RFID-læseren. `OnRfidRead(int id)` fyrer `RfidDetectedEvent` med id'et i `RfidDetectedEventArgs`. Læseren har ingen logik – al vurdering af om id'et er korrekt ligger i `StationControl`.

### Display (`IDisplay`)
Displayet har to områder, jf. kravspecifikationen: brugerinstruktioner (`ShowStationMessage`) og ladestatus (`ShowChargingMessage`). De simuleres ved at skrive til konsollen med præfikserne `[Station]` og `[Ladning]`, så man kan se, hvilket område en besked hører til.

### LogFile (`ILogFile`)
Logger låsning og oplåsning som én linje pr. hændelse:

```
14:05:09: Skab låst med RFID: 1234
14:07:42: Skab låst op med RFID: 1234
```

De to svært kontrollerbare afhængigheder – systemuret og filsystemet – er indkapslet bag `ITimeProvider` og `IFileWriter`. `LogFile` sammensætter kun teksten; de rigtige implementeringer `SystemTimeProvider` og `FileWriter` er tynde wrappers om `DateTime.Now` og `File.AppendAllText`.

Tidsstemplet formateres med `CultureInfo.InvariantCulture`. Uden det bliver formatet `HH:mm:ss` til `14.05.09` på en maskine med dansk sprogindstilling, fordi `:` i .NET er kulturens tidsseparator. Det blev opdaget under test og er dækket af en test med `[SetCulture("da-DK")]`.

RFID-læseren leverer id'et som `int`, mens `ILogFile` tager en `string`. `StationControl` konverterer med `id.ToString()`, så logfilen ikke er bundet til, hvordan et RFID-id er repræsenteret.

### Test af boundary-klasserne
| Klasse | Testteknik |
| --- | --- |
| `Door` | Events fanges med lambda-handlers og tælles. Alle kombinationer af låst/ulåst og åben/lukket testes, inkl. at der ikke fyres events (ZOMBIE: Zero/One/Many). |
| `RfidReader` | Event-indhold og sender kontrolleres. Id'et testes med EP/BVA: `0`, `1`, `-1`, `int.MaxValue`, `int.MinValue`. |
| `Display` | Konsoloutput omdirigeres med `Console.SetOut(new StringWriter())` og sammenlignes eksakt. Konsollen genskabes i `[TearDown]`. |
| `LogFile` | `ITimeProvider` og `IFileWriter` fakes med NSubstitute. Tidsstemplet testes med BVA (`00:00:00`, `23:59:59`, 24-timers format, foranstillede nuller) og dansk kultur. `Received.InOrder` sikrer rækkefølgen af linjer. |

`SystemTimeProvider` og `FileWriter` er markeret `[ExcludeFromCodeCoverage]`: de indeholder ingen logik, og en unit test af dem ville teste .NET's ur og filsystem frem for vores kode.

## 5. Teststrategi
*(Fælles: unit-test af hver klasse isoleret med fakes, NUnit + NSubstitute, valg af testcases med BVA, EP og ZOMBIE.)*

## 6. Coverage
*(Mohamad: hvordan coverage måles med coverlet/ReportGenerator, opnået coverage og begrundelse for undtagelser.)*

## 7. Continuous Integration
*(Mohamad: pipeline-opsætning i `.gitlab-ci.yml`, testrapport og coverage-rapport i GitLab.)*

## 8. Refleksion over designet
*(Person 2, udkast: hvorfor events, fordele og ulemper ved designet.)*

## 9. Arbejdsfordeling
*(Fælles: hvordan og hvorfor arbejdet blev fordelt, fordele og ulemper.)*

## 10. Refleksion over fælles repository og CI
*(Fælles: observationer, fordele og ulemper.)*
