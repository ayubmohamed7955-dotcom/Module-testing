# Dokumentation – Ladeskab

[← Tilbage til forsiden](../README.md)

Ladeskabssystemet er delt i **controller-klasser**, der indeholder logikken, og **boundary-klasser**, der står for kontakten med hardware og omverden. Controllerne kender kun interfaces, så al hardware, filsystem, ur og konsol kan erstattes af fakes i unit tests.

Dokumentationen er delt i én fil pr. del af systemet. Denne side giver overblikket og samler teststrategi, CI og de fælles refleksioner.

| Del | Fil | Indhold |
| --- | --- | --- |
| StationControl | [StationControl.md](StationControl.md) | Klassediagram for hele systemet, tilstandsdiagram, sekvensdiagram for låsning og oplåsning, test og designrefleksion |
| ChargeControl | [ChargeControl.md](ChargeControl.md) | Sekvensdiagram for opladning, strømintervaller, test |
| Boundary-klasser | [Boundary.md](Boundary.md) | Door, RfidReader, Display og LogFile: diagrammer, test og designrefleksion |

Billeder ligger i `docs/images/`. Diagrammerne er skrevet i Mermaid og vises direkte på GitLab.

## Indhold
1. [Overblik over designet](#1-overblik-over-designet)
2. [Teststrategi](#2-teststrategi)
3. [Coverage](#3-coverage)
4. [Continuous Integration](#4-continuous-integration)
5. [Refleksion over designet](#5-refleksion-over-designet)
6. [Arbejdsfordeling](#6-arbejdsfordeling)
7. [Refleksion over fælles repository og CI](#7-refleksion-over-fælles-repository-og-ci)

---

## 1. Overblik over designet

Systemet består af:

- **StationControl**: controller for selve skabet. Lytter på events fra døren og RFID-læseren og følger tilstandsdiagrammet (Available, DoorOpen, Locked). Se [StationControl.md](StationControl.md).
- **ChargeControl**: controller for opladningen. Lytter på strømmålinger fra USB-laderen og viser ladestatus. Se [ChargeControl.md](ChargeControl.md).
- **Boundary-klasser**: `Door`, `RfidReader`, `Display` og `LogFile` (med `SystemTimeProvider` og `FileWriter`) samt den udleverede `UsbChargerSimulator`. Se [Boundary.md](Boundary.md).
- **Konsolappen**: sætter klasserne sammen og simulerer hardwaren via tastaturet.

Klassediagrammet for hele systemet findes i [StationControl.md](StationControl.md#klassediagram).

## 2. Teststrategi
*(Fælles: unit-test af hver klasse isoleret med fakes, NUnit + NSubstitute, valg af testcases med BVA, EP og ZOMBIE. Detaljerne for hver klasse står i dens egen fil.)*

## 3. Coverage
*(Mohamad: hvordan coverage måles med coverlet/ReportGenerator, opnået coverage og begrundelse for undtagelser.)*

## 4. Continuous Integration
*(Mohamad: pipeline-opsætning i `.gitlab-ci.yml`, testrapport og coverage-rapport i GitLab.)*

## 5. Refleksion over designet
*(Fælles: de overordnede valg – events, constructor injection og interfaces. Klassespecifikke refleksioner står i de enkelte filer.)*

## 6. Arbejdsfordeling
*(Fælles: hvordan og hvorfor arbejdet blev fordelt, fordele og ulemper.)*

## 7. Refleksion over fælles repository og CI
*(Fælles: observationer, fordele og ulemper.)*
