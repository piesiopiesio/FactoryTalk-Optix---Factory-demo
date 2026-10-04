# BACKLOG

Format: `- [ ] [S|M] Tytuł — kryterium akceptacji`. Kolejność w `## Next` = priorytet.
`[user]` = od użytkownika (agent nie usuwa), `[agent]` = propozycja agenta, `(blocked)` = wstrzymane.

## Next
<!-- [studio] = wymaga Studio (element z Template Library dodaje się w Studio); robi sesja połączona z komputerem Maćka -->
- [ ] [S] Mikroprzestoje < 30 s — straty wydajności widoczne w OEE (Performance spada o 2–5 pp)
- [ ] [M] Straty OEE per stacja (Pareto) — dane w trace-summary + wykres w `Line1`
- [ ] [M] Model zmian (3 zmiany, planowane przestoje) — czas planowany w OEE
- [ ] [M] SKU 0,5 L / 1,5 L z przezbrojeniem — czasy cyklu z receptury w factory.json
- [ ] [M] DataLogger + `TrendsScreen` w FactoryBuilder — logger OEE/throughput/tankLevel i trend
- [ ] [M] Priorytety alarmów wg przewodnika HMI — priorytet 1–4 w `FaultCatalog` (np. kurtyna świetlna = Pilny), kolor i numer w ikonie
- [ ] [S] Potwierdzanie alarmów — stan niepotwierdzony (miganie ramki) i potwierdzony (stała ramka) w podglądzie i Optix

## Later
- [ ] [S] [studio] Automatyczne wylogowanie po bezczynności (IdleTimeoutLogic z Template Library)
- [ ] [L] Model stanów PackML (ISA-TR88) dla linii: Stopped/Idle/Execute/Suspended/Held/Aborted + ikony „States” z Template Library
- [ ] [M] [studio] Raport zmianowy PDF (moduł Report; wzór: FactoryTalk-Optix/Training_Reports) — OEE, produkcja, awarie
- [ ] [M] [studio] Pareto przestojów z historii alarmów (wzór: FactoryTalk-Optix/Optix_Sample_ParetoAlarmChart)
- [ ] [M] [studio] Receptury SKU przez RecipesEditor/RecipeX (łączy się z pozycją „SKU 0,5 L / 1,5 L”)
- [ ] [S] [studio] Zegar w nagłówku (ClockLogic) i powiadomienie „toast” o nowej awarii (wzór: Optix_Sample_ToastNotification)
- [ ] [L] Sterownik zamiast symulacji: RA EtherNet/IP + Logix Emulate (tagi PLC w miejsce SimulationLogic, ta sama warstwa UI)
- [ ] [M] LineScreen oparty o alias (jeden ekran dla wielu linii; Optix 1.8 `SetDynamicLinkToAlias`)
- [ ] [M] Faceplate w Optix (okno dialogowe z sygnałami stacji) generowany przez FactoryBuilder
- [ ] [M] Strefa Magazyn: palety z paletyzatora trafiają do regału (AGV jako taśma logiczna)
- [ ] [M] Strefa Media: sprężone powietrze i energia per linia (kWh/1000 butelek)
- [ ] [M] Druga linia (procesowa: mieszalnik + CIP) w hali
- [ ] [M] Rozgałęzienia taśm (bufor boczny, odrzut z wizyjnej na osobną taśmę)
- [ ] [M] Serwer OPC UA + MQTT publikujący KPI hali
- [ ] [M] Faceplate z zakładkami Home / Diagnostyka / Ustawienia / Alarmy (przewodnik HMI), rozmiar = kontrolki × 50 + 10
- [ ] [S] Sparkline OEE i wydajności na poziomie 1 (hala), bar graph poziomu zbiornika z limitami
- [ ] [S] Pasek przycisków poziomu 2/3 w nagłówku Optix (nawigacja wg przewodnika HMI)
- [ ] [S] [agent] `snapshot.py`: mini-mapa stacji w kafelku linii na obrazie hali (jak w artboardzie `Main`)
- [ ] [S] [agent] Wejście/wyjście butelek między klatkami w `Line1` (pojawianie się na 0 i znikanie na 1 w trakcie interpolacji, bez skoku co 2 s)
- [ ] [S] [agent] `snapshot.py --sub 0.5`: klatka pośrednia taśm (ta sama reguła co `beltAt` w artboardzie), żeby agent widział interpolację
- [ ] [S] [agent] Priorytet alarmu w podglądzie: `P1–P4` przy awarii w faceplate `Line1` i w nagłówku (z `AlarmPriority` w layout.json)
- [ ] [S] [agent] Studio: sprawdzić, czy `Severity` ustawione przez `GetVariable("Severity")` widać w AlarmGrid; jeśli jest właściwość `DigitalAlarm.Severity` — dopisać do stubów i użyć jej
- [ ] [S] [agent] [studio] Sprawdzić w emulatorze, że AdvancedTrend pokazuje DisplayName pióra (nie BrowseName); jeśli nie — ustawić tytuł pióra w widgecie
- [ ] [S] [agent] Ta sama lista `TrendPens` w podglądzie: artboard trendu (OEE, przepustowość, zbiornik) z `preview.json`
- [ ] [S] [agent] [studio] Sprawdzić w emulatorze format „2 148” / „74,6” po zalogowaniu (LocaleId kont demo) i bez logowania (Locales projektu = pl-PL)
- [ ] [S] [agent] Polski format liczb w podglądzie (`Intl.NumberFormat('pl-PL')` w `Main`/`Line1`/`Status` i w `snapshot.py`), spójnie z Optix
- [ ] [S] [agent] [studio] Sprawdzić w emulatorze (i w kliencie web), że klik w kafel/„Ekran linii” na hali otwiera zakładkę linii tylko w tej sesji; jeśli `openTab` ekranu nie jest per sesja — przenieść żądanie do zmiennej sesji
- [ ] [S] [agent] Link „Ekran linii” w podglądzie także z całego kafla (artboard `Main`), spójnie z Optix
- [ ] [S] [agent] Optix: komunikat informacyjny (nie alarm) przy `LowTank` / uzupełnianiu zbiornika — np. wpis w liście zdarzeń `LinePanel` i ikona „i” przy napełniarce
- [ ] [S] [agent] Podgląd `Line1`: pasek poziomu zbiornika napełniarki z progami `lowPct`/`refillToPct` (z params w layout.json)
- [ ] [S] [agent] Optix: ostrzeżenie (nie alarm) przy niskim zapasie etykiet (`LabelStock` < 10 %) w `LinePanel`, żeby operator przygotował rolkę
- [ ] [S] [agent] `Sim.Cli` Checks: kontrola „przestoje planowe występują” (≥ 1 Maintenance na PAL, FILL, LAB w 30 min), żeby zmiana parametrów ich nie wyłączyła
- [ ] [S] [agent] `Alarms`: klik w wiersz przenosi do `Line1` na chwilę zdarzenia (parametr w URL / wspólny stan), z zaznaczoną stacją
- [ ] [S] [agent] Zakładka „Alarmy” także w nagłówku `Main` i `Status` (spójna nawigacja podglądu)
- [ ] [S] [agent] [studio] Sprawdzić w emulatorze przycisk „Wstrzyknij awarię” w faceplate (wyłączony dla operatora, alarm P… pojawia się, Kasuj po naprawie) i metodę `InjectFault` w SimulationLogic
- [ ] [S] [agent] `Sim.Cli --inject L1/LAB@600`: scenariusz demo z awarią wstrzykniętą w zadanym czasie (przebieg nadal deterministyczny), żeby podgląd pokazał awarię testową

## Blocked

## Done
- [x] 2026-10-04 Wstrzykiwanie awarii: `Cmd.InjectFault` w Core (tylko urządzenie, kod 1, naprawa = MTTR lub 30 s, bez losowania,
  zdarzenie „(wstrzyknięta)”), bit `cmdFault` + przycisk „Wstrzyknij awarię” w faceplate Optix (utrzymanie ruchu),
  `SimulationLogic.InjectFault(path)` [ExportMethod]; w podglądzie `Line1` przycisk skacze do najbliższej awarii stacji; 3 testy
- [x] 2026-10-03 Artboard `Alarms` (pod `Line1`): awarie i obsługi planowe sparowane w wiersze (początek, koniec, czas trwania,
  stacja, rodzaj, opis) + informacje (`LowTank`); filtry, kafle (liczba/łączny czas awarii, MTTR, przestoje planowe); zakładka i link z `Line1`
- [x] 2026-10-02 Wymiana rolki etykiet: pusta rolka (`rollLabels` 1500) → `MaintenanceStarted` „wymiana rolki etykiet”,
  `Maintenance` etykieciarki przez `rollChangeS` (45 s) → `MaintenanceEnded` „nowa rolka”; widoczne w OEE i zdarzeniach; test
- [x] 2026-09-30 Cykl uzupełniania zbiornika napełniarki: zamiast stałego dopływu partie — poniżej `lowPct` (20 %) zdarzenie
  `LowTank` + `Maintenance` („uzupełnianie zbiornika”) do `refillToPct` (95 %) z `refillLps` 12 L/s; widoczne w OEE i zdarzeniach; test
- [x] 2026-09-29 Kliknięcie kafla linii na hali przełącza zakładkę na ekran linii: kafel + przycisk „Ekran linii” zapisują indeks
  zakładki (Core `NavTabs`) do `HallScreen/openTab`, `AccessLogic` przełącza `MainNav.CurrentTabIndex` w tej sesji (≤ 0,5 s)
- [x] 2026-09-28 Separator tysięcy po polsku w Optix: konta demo dostają `LocaleId = pl-PL` (CreateDemoUsers), StringFormatter
  formatuje wg locale sesji; dla sesji bez logowania krok w Studio (Locales projektu) opisany w docs/studio-setup.md
- [x] 2026-09-27 Czytelne nazwy piór trendu: `TrendPens` w Core (kolumna = dotychczasowa BrowseName, np. `L1_oee`;
  pióro = DisplayName pl-PL, np. „L1 OEE [%]”, „L1 Napełniarka: Zbiornik [%]”); podpisy KPI linii i zapełnienia taśm; test
- [x] 2026-09-26 [studio] Logowanie (LoginForm, użytkownik w nagłówku), role Operatorzy / UtrzymanieRuchu blokujące przyciski linii, Stop z potwierdzeniem — sprawdzone w emulatorze
- [x] 2026-09-26 Priorytety alarmów 1–4 (ISA-18.2): `AlarmPriority` w Core (paletyzator Pilny, napełniarka/zakręcarka Wysoki,
  taśmy/kartoniarka/wizyjna Średni, podajnik/etykieciarka Niski) → `Severity` 900/700/500/300 i prefiks `P1…` w komunikacie alarmu; test
- [x] 2026-09-26 Pakiet bibliotek 1: ISAStyleSheet, AlarmBanner w nagłówku, zakładka Alarmy (aktywne z Ack/Confirm + historia
  z EmbeddedDatabase), zakładka Trendy (AdvancedTrend, 16 zmiennych w DataLogger1) — sprawdzone w emulatorze
- [x] 2026-09-25 Pierwsze uruchomienie w Studio (Factory_demo, Optix 1.7.5.13): FactoryBuilder generuje typy, model, alarmy,
  ekrany Hala/Linia 1 z nagłówkiem i zakładkami; emulator: dane na żywo, Start/Stop, Czas ×10, awarie + Kasuj — sprawdzone
- [x] 2026-09-25 Płynna animacja taśm w `Line1` — odtwarzanie 10 fps, interpolacja kinematyczna (prędkość, podziałka), test kolejności pozycji
- [x] 2026-09-25 Szkielet: Core + 7 stacji + taśmy, Sim.Cli, testy, warstwa Optix na stubach, podgląd Design (Hala, Linia 1, Status)
