# KinTAP – Kinder-Tages-Aufgaben-Planer (Desktop)

KinTAP ist eine Desktop-Anwendung, die Kindern hilft, ihren Alltag zu strukturieren. Eltern legen Aufgaben an, Kinder haken sie ab, und für erledigte Aufgaben gibt es Punkte, die im Belohnungs-Shop eingelöst werden können.

Die App entstand als Projekt im 3. Semester meiner Ausbildung zum Fachinformatiker für Anwendungsentwicklung. Sie ersetzte in unserer Familie die Plantafel mit Magnet- und Klettbildern, von der ständig Teile verloren gingen. Besonders Kinder aus dem Autismus-Spektrum oder mit ADHS profitieren von einer klaren, verlässlichen Tagesstruktur.

> **Hinweis:** Dieses Repository ist der erste Prototyp. Später habe ich KinTAP für meine IHK-Abschlussarbeit als geräteunabhängige Web-App neu entwickelt (HTML, CSS, Vanilla JavaScript, PHP und SQLite auf einem Raspberry Pi), damit die App auch auf Tablets und Smartphones läuft.

## Funktionen

**Kinderbereich**
- Profilauswahl beim Start („Wer bist Du?“)
- Liste der Aufgaben für heute, farblich nach Priorität markiert
- Aufgabe mit „Fertig!“ als erledigt melden – die Eltern bestätigen danach
- Punktestand und Belohnungs-Shop, offene Belohnungen im Blick
- Eigenes Profilbild (Avatar) und eigene Hintergrundfarbe wählen

**Elternbereich (mit Passwort geschützt)**
- Kinder anlegen, bearbeiten und löschen, Foto auswählen
- Aufgaben erstellen mit Titel, Beschreibung, Punkten und Priorität (Wichtig, Wünschenswert, Frei)
- Wiederholung: einmalig (optional mit festem Datum), täglich oder wöchentlich an ausgewählten Wochentagen
- Optionales Zeitfenster (Beginn und Ende)
- Aufgaben Kindern zuweisen; die App erzeugt daraus jeden Tag automatisch die passenden Aufgaben
- Erledigte Aufgaben bestätigen (Punkte werden gutgeschrieben) oder ablehnen
- Belohnungen anlegen und verwalten, Übergabe an das Kind bestätigen
- Passwort ändern

## Technik

| Bereich | Umsetzung |
|---|---|
| Sprache / Plattform | C#, .NET 9 |
| Oberfläche | [Avalonia UI](https://avaloniaui.net/) 11 (AXAML), Fluent-Theme |
| Architektur | Aufteilung in Model, View, Controller, Services und Persistence (je ein eigenes Projekt) |
| MVVM-Hilfen | CommunityToolkit.Mvvm |
| Datenhaltung | JSON-Dateien im Benutzerordner |

Ein paar Details, auf die ich damals stolz war:
- **Wochentage als Bitmaske** (`[Flags]`-Enum): Mo = 1, Di = 2, Mi = 4 … So = 64. Mo + Mi + Fr ergibt zum Beispiel 21.
- **Vorlagen und Abonnements:** Eine Aufgabe wird einmal als Vorlage angelegt und einem Kind „abonniert“. Beim Öffnen erzeugt der `DataService` die Aufgaben für den aktuellen Tag und verhindert Duplikate.
- **Bestätigungsablauf:** Offen → vom Kind erledigt → von den Eltern bestätigt. Erst dann gibt es Punkte.

### Projektstruktur

```
KinTAP.sln
├── KinTAP.Model/        Datenklassen (Kind, Aufgabe, Belohnung, Abo …)
├── KinTAP.View/         Fenster und Ansichten (AXAML), Converter, Assets
├── KinTAP.Controller/   Logik für Eltern- und Kinderbereich
├── KinTAP.Services/     Datendienst, Aufgabengenerierung, Passwort
└── KinTAP.Persistence/  Laden und Speichern als JSON
```

## Starten

Voraussetzung: [.NET 9 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/anatolij-imgrund/KinTAP_Desktop.git
cd KinTAP_Desktop
dotnet run --project KinTAP.View
```

Alternativ `KinTAP.sln` in Visual Studio oder JetBrains Rider öffnen und `KinTAP.View` als Startprojekt wählen.

- **Standardpasswort für den Elternbereich:** `KinTAP2025` (kann in der App geändert werden)
- **Speicherort der Daten:** `%APPDATA%\KinTAP\` unter Windows (`~/.config/KinTAP/` unter Linux). Zum Zurücksetzen einfach den Ordner löschen.

## Bekannte Grenzen

Das ist ein Lernprojekt, und das sieht man dem Code auch an. Unter anderem:
- Die App läuft nur auf einem Rechner; Eltern und Kinder teilen sich ein Gerät.
- Das Passwort liegt im Klartext in einer Datei.
- Es gibt keine automatischen Tests.

Genau diese Grenzen waren der Anlass für die spätere Web-Version.

## Schriftarten

Die mitgelieferten Schriftarten stehen unter der [SIL Open Font License 1.1](https://openfontlicense.org/) (Lizenztexte in den jeweiligen Ordnern unter `KinTAP.View/Assets/Fonts/`):
- Grandstander – The Grandstander Project Authors
- Itim – Cadson Demak
- Playpen Sans – The Playpen Sans Project Authors
- Shantell Sans – The Shantell Sans Project Authors

## Autor

Imgrund Anatolij – entstanden während der Ausbildung zum Fachinformatiker für Anwendungsentwicklung.
