# JOURNAL

Najnowszy wpis na górze. Maks. 14 wpisów; starsze w `docs/archive/`.

## 2026-09-26 (południe, sesja przy Studio)
- Faceplate stacji (poziom 3): klik w kafel stacji → ramka nad schematem (`StationDetail`), zamykana „Zamknij”; `Line/selectedStation`.
- Grafiki z Template Library (GraphicElements): Bottle1, Carton, PalletBoxSide, PhotoEyeSensorSide — płasko (nakładki 3D ukryte),
  ruch tylko z danymi: poziom w butelce = napełnienie, głowica zakręcarki schodzi z postępem cyklu, etykieta owija się,
  linia skanu wizyjnej, karton = butelki w kartonie, stos na palecie = kartony. „Kasuj awarię” w faceplacie tylko dla UR.
- Budżet kontekstu źródeł podniesiony do 68k (warstwa Optix urosła; rdzeń nadal 12k).

## 2026-09-26 (przedpołudnie, sesja przy Studio)
- Logowanie i role gotowe i sprawdzone w emulatorze (podgląd CDP): bez logowania przyciski linii nieaktywne; `operator` — Start/Stop/tempo,
  „Kasuj awarie” wyłączone; `serwis` — także kasowanie. Stop otwiera nakładkę Zatrzymaj/Anuluj (`stopRequest`), obie ścieżki działają.
- Konta: Build zakłada grupy i użytkowników z `demo-users.json` (katalog projektu); hasła ustawia runtime (`SimulationLogic.Start` →
  `Session.ChangePassword`), bo w Studio ta metoda nie istnieje. Hasła testowe w pamięci projektu Claude, nie w repo.
- Priorytety alarmów z przebiegu dziennego sprawdzone: `GetVariable("Severity")` zwracało null → teraz `alarm.Severity`; P1 = 900 potwierdzone.
- Pułapki Studio: nowa metoda [ExportMethod] pojawia się w menu Execute dopiero po Ctrl+S w Studio (nie po samej zmianie pliku);
  automatyczna kompilacja po wgraniu plików bywa zawodna → Ctrl+S. Czarne zrzuty ekranu po starcie PC: pomógł Win+Ctrl+Shift+B.

## 2026-09-26 (rano, sesja z Maćkiem z telefonu)
- Przegląd przebiegu dziennego: priorytety alarmów OK (15/15); Severity do sprawdzenia w Studio przy najbliższym Execute Build.
- `gen_status`: źródło „github” tylko przy zdalnym `github` (kopia z canvasu miała origin = bundle), dziennik czyta nagłówki z dopiskiem.
- Zdalny dostęp do PC: wybrane uśpienie zamiast wyłączania + codzienna pobudka 07:00 na 60 min
  (`tools/windows/setup-sleep-wake.ps1`, `keep-awake.ps1`, opis `tools/windows/README.md`). Czeka na instalację i test przy PC.

## 2026-09-26 (przebieg dzienny)
- Zadanie: priorytety alarmów wg ISA-18.2. Nowy `AlarmPriority` (Core, 1 Pilny … 4 Niski) nadpisywany per typ urządzenia;
  `AlarmGenerator` ustawia `Severity` (900/700/500/300) i dopisuje `P1…P4` do komunikatu (priorytet nie tylko kolorem).
- Założenie: jeden alarm na urządzenie, więc priorytet jest per urządzenie, nie per kod awarii (to zostaje w pozycji [M] „priorytet w FaultCatalog”).
  `Severity` zapisywane przez `GetVariable("Severity")` + `new UAValue(ushort)` — właściwość `DigitalAlarm.Severity` niesprawdzona w CheatSheet; do weryfikacji w Studio.
- Test `AlarmPrioritiesFollowIsa182` (15/15). KPI bez zmian (30 min, seed 42): OEE 74,6 %, 71,6 butelki/min średnio, 9 awarii.
- GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. Następne: czytelne nazwy piór trendu.

## 2026-09-26 (noc, sesja z Maćkiem przy Studio)
- Przegląd bibliotek: wbudowana Template Library 1.7.5.13 (~45 widgetów, grafiki, skrypty, 4 style), 134 repo FactoryTalk-Optix,
  biblioteki urządzeń i pakiet OEE+PackML z Innovation Center. Rekomendacje w BACKLOG (znacznik [studio]).
- Wdrożone: ISA Style Sheet (zakładki/przyciski szare), AlarmBanner, zakładka Alarmy (AlarmGrid + historia z AlarmsEventLogger1
  na EmbeddedDatabase1), zakładka Trendy (AdvancedTrend na DataLogger1). Alarmy teraz potwierdza operator (bez AutoAck).
- Test: awarie widoczne w banerze i siatce, Acknowledge All działa, historia zapisuje zdarzenia, trend rysuje 16 piór.

## 2026-09-25 (wieczór, sesja z Maćkiem przy Studio)
- ftx-mcp podłączone (most 1.0.7, emulator, CDP). Kod eksportowany `tools/export_optix.py` do `Factory_demo/ProjectFiles`;
  kompilacja na prawdziwym Optix 1.7 przez `optix_build_check` (0 błędów). Stuby dopasowane do sprawdzonego API.
- Nowa warstwa ekranów: `Ui`, `HallView` (mini-mapa stacji), `LineView`, `StationTable`, `LinePanel`, `WindowGenerator`.
  Core: `StateText` (stan tekstem), `Hall.ActiveFaults`, `[Signal(Label=…)]` dla podpisów.
- Pułapki znalezione w Studio (opis w docs/studio-setup.md): klasa NetLogic w katalogu głównym NetSolution, sufiks `Type`
  dla typów (proxy Studio przesłaniały `Line`/`Hall`), węzeł NetLogic z kodu nie ma metod → polecenia jako bity + `VariableCommands.Set`.
- Test w emulatorze: dane płyną, Stop/Start, ×10, awaria FEED → lista awarii + lampka w nagłówku → Kasuj działa.
- Następne: locale pl-PL (separator tysięcy), ekran Alarmy, klik kafla linii → zakładka.

## 2026-09-25
- Zadanie: płynna animacja taśm w `Line1`. Odtwarzanie tyka co 100 ms (10 fps) z ułamkiem klatki `sub`; kropki przesuwane regułą `Conveyor.Step` (prędkość × SpeedPct, limit = poprzednia − podziałka), tylko w stanie Praca/Zablokowana.
- `layout.json` eksportuje `pitchM` i `speedMps` taśm; nowy test `BeltPositionsAreHeadFirstAndPitched` (14/14). Sprawdzenie w node na całym przebiegu: 16 200 klatek pośrednich, 0 naruszeń podziałki/zakresu.
- Założenie: klatki co 2 s nie pozwalają śledzić pojedynczych butelek (ruch 0,8 m/klatkę ≫ podziałka 0,08 m), więc interpolacja jest kinematyczna, a butelki wchodzące/wychodzące pojawiają się dopiero w następnej klatce (propozycja w Later).
- KPI (30 min, seed 42): OEE 74,6 %, średnio 71,6 butelki/min (96/min w ostatniej minucie), 9 awarii. GitHub niedostępny → tylko kopia w canvasie.
- Poprawka `gen_status.py`: backlog na artboardzie `Status` był pusty (split trafiał na „`## Next`” we wstępie BACKLOG.md).
- Następne: cykl uzupełniania zbiornika napełniarki (`LowTank`, `Maintenance`).

## 2026-09-25
- Start projektu: manifest `factory.json`, rdzeń symulacji (7 stacji, 6 taśm, OEE), `Sim.Cli`, 13 testów.
- Warstwa Optix (FactoryBuilder, SimulationLogic, OptixBinder) kompiluje się na stubach; czeka na test w Studio.
- Podgląd w Claude Design: Hala, Linia 1 (odtwarzanie przebiegu, faceplate), Stan projektu.
- Przebieg 30 min, seed 42: OEE ok. 75%, ok. 72 butelki/min średnio, 9 awarii.
- Styl przestawiony na Rockwell Process HMI Style Guide (ISA-101): `docs/hmi-style.md`, `design/theme.json`.
- Brak dostępu sesji do GitHuba → kopia repo w canvasie (`repo.bundle.b64`); `tools/sync_remote.sh` połączy przy pierwszej okazji.
