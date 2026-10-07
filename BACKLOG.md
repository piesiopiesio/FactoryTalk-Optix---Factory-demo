# BACKLOG

Format: `- [ ] [S|M] Tytuł — kryterium akceptacji`. Kolejność w `## Next` = priorytet.
`[user]` = od użytkownika (agent nie usuwa), `[agent]` = propozycja agenta, `(blocked)` = wstrzymane.

## Next
<!-- [studio] = wymaga Studio (element z Template Library dodaje się w Studio); robi sesja połączona z komputerem Maćka -->
<!-- [help X.Y] = klocek z pomocy Optix; API i pułapki: docs/reference/optix-help-klocki.md; kolejność = plan (dokument planu, zakładka „Klocki z pomocy Optix”) -->
- [ ] [S] [help 1.4] Locale: `LocaleId` pl-PL w sesji + `TrendPen.Title` en-US/pl-PL — bez logowania „69,7 %”, legenda „L1 OEE [%]”
- [ ] [S] [help 1.3b] [studio] Po potwierdzeniu w web, że klik w kafel stacji i linii działa (MouseUp): usunąć przyciski „Szczegóły” i „Ekran linii”
- [ ] [S] [help 2.1] Wylogowanie po bezczynności przez UISession (`IdleTimeoutEnabled`, 5 min, `IdleTimeoutEvent` → Logout) — zastępuje IdleTimeoutLogic
- [ ] [S] [help 2.2] Kafel hali wywołuje `MainNav.ChangePanelByTabIndex` — bez `openTab` i pętli w AccessLogic, przełączenie od razu
- [ ] [M] [help 2.3] `Enabled` przycisków z grup sesji (zamiast pętli AccessLogic co 0,5 s) — anonim/operator/serwis jak dziś
- [ ] [M] [help 2.4] Okno stacji jako Dialog z aliasem `{Station}` + `UICommands.OpenDialog` (typ bazowy stacji) — zamiast 7 ukrytych `Detail_*`
- [ ] [S] [help 2.5] Ikona priorytetu P1–P4 (MultistateImage) przy stacji + `Blink` do potwierdzenia (System › Blink), też w podglądzie — ISA-101
- [ ] [S] [help 2.6] Sparkline OEE na kaflu hali (pasmo celu), LinearGauge zbiornika ze strefami `lowPct`/`refillToPct`, TrendThreshold celu OEE
- [ ] [S] [help 2.7] Budżet węzłów: `{0}` bez StringFormatter, mniej handlerów; pomiar Nodes Counter przed/po — ekran linii −30 % węzłów
- [ ] [S] [help 3.1] HistogramChart Pareto z `oeeLossS` na ekranie linii (Optix) — zgodny z panelem w `Line1`
- [ ] [M] [help 3.2] Awarie z historii alarmów: SQL GROUP BY (liczba) + czasy parowane w NetLogic — tabela na zakładce Alarmy
- [ ] [M] [help 3.3] Raport zmianowy PDF (Report + `GeneratePdf(…, "pl-PL")`) — PDF po kliknięciu, KPI + stacje + awarie
- [ ] [M] SKU 0,5 L / 1,5 L z przezbrojeniem — czasy cyklu z receptury w factory.json
- [ ] [M] [help 3.4] Receptury SKU (RecipeSchema na osobnej bazie, RecipeEditor, `TransferFromStoreToTarget`) — po pozycji „SKU 0,5 L / 1,5 L”
- [ ] [M] [help 4.1] Serwer OPC UA: `NodesToPublish` = Model/Factory, `UseNodePathInNodeIds`, bez szyfrowania — UaExpert widzi KPI i InjectFault
- [ ] [M] [help 4.2] MQTT: wbudowany broker + MQTTPublisher z DataLogger1 — MQTT Explorer widzi KPI co 1 s
- [ ] [S] [help 4.3] Rozpoznanie: typy Weihenstephan (companion specs) dla napełniarki — notatka, co Build wygeneruje z C#
- [ ] [S] [help 4.4] Rozpoznanie: `TemplateLibrary.ImportLibraryItem` w Build (wymaga „Show feature preview”) — brakujący element dodany sam
- [ ] [M] Model zmian (3 zmiany, planowane przestoje) — czas planowany w OEE
- [ ] [M] DataLogger + `TrendsScreen` w FactoryBuilder — logger OEE/throughput/tankLevel i trend

## Later
- [ ] [L] Model stanów PackML (ISA-TR88) dla linii: Stopped/Idle/Execute/Suspended/Held/Aborted + ikony „States” z Template Library
- [ ] [S] [studio] Zegar w nagłówku (ClockLogic) i powiadomienie „toast” o nowej awarii (wzór: Optix_Sample_ToastNotification)
- [ ] [L] Sterownik zamiast symulacji: RA EtherNet/IP + Logix Emulate (tagi PLC w miejsce SimulationLogic, ta sama warstwa UI)
- [ ] [M] LineScreen oparty o alias (jeden ekran dla wielu linii; Optix 1.8 `SetDynamicLinkToAlias`)
- [ ] [M] Strefa Magazyn: palety z paletyzatora trafiają do regału (AGV jako taśma logiczna)
- [ ] [M] Strefa Media: sprężone powietrze i energia per linia (kWh/1000 butelek)
- [ ] [M] Druga linia (procesowa: mieszalnik + CIP) w hali
- [ ] [M] Rozgałęzienia taśm (bufor boczny, odrzut z wizyjnej na osobną taśmę)
- [ ] [M] Faceplate z zakładkami Home / Diagnostyka / Ustawienia / Alarmy (przewodnik HMI), rozmiar = kontrolki × 50 + 10
- [ ] [S] Pasek przycisków poziomu 2/3 w nagłówku Optix (nawigacja wg przewodnika HMI)
- [ ] [S] [agent] `snapshot.py`: mini-mapa stacji w kafelku linii na obrazie hali (jak w artboardzie `Main`)
- [ ] [S] [agent] Wejście/wyjście butelek między klatkami w `Line1` (pojawianie się na 0 i znikanie na 1 w trakcie interpolacji, bez skoku co 2 s)
- [ ] [S] [agent] `snapshot.py --sub 0.5`: klatka pośrednia taśm (ta sama reguła co `beltAt` w artboardzie), żeby agent widział interpolację
- [ ] [S] [agent] Priorytet alarmu w podglądzie: `P1–P4` przy awarii w faceplate `Line1` i w nagłówku (z `AlarmPriority` w layout.json)
- [ ] [S] [agent] Ta sama lista `TrendPens` w podglądzie: artboard trendu (OEE, przepustowość, zbiornik) z `preview.json`
- [ ] [S] [agent] Polski format liczb w podglądzie (`Intl.NumberFormat('pl-PL')` w `Main`/`Line1`/`Status` i w `snapshot.py`), spójnie z Optix
- [ ] [S] [agent] Link „Ekran linii” w podglądzie także z całego kafla (artboard `Main`), spójnie z Optix
- [ ] [S] [agent] Optix: komunikat informacyjny (nie alarm) przy `LowTank` / uzupełnianiu zbiornika — np. wpis w liście zdarzeń `LinePanel` i ikona „i” przy napełniarce
- [ ] [S] [agent] Podgląd `Line1`: pasek poziomu zbiornika napełniarki z progami `lowPct`/`refillToPct` (z params w layout.json)
- [ ] [S] [agent] Optix: ostrzeżenie (nie alarm) przy niskim zapasie etykiet (`LabelStock` < 10 %) w `LinePanel`, żeby operator przygotował rolkę
- [ ] [S] [agent] `Sim.Cli` Checks: kontrola „przestoje planowe występują” (≥ 1 Maintenance na PAL, FILL, LAB w 30 min), żeby zmiana parametrów ich nie wyłączyła
- [ ] [S] [agent] `Alarms`: klik w wiersz przenosi do `Line1` na chwilę zdarzenia (parametr w URL / wspólny stan), z zaznaczoną stacją
- [ ] [S] [agent] Zakładka „Alarmy” także w nagłówku `Main` i `Status` (spójna nawigacja podglądu)
- [ ] [S] [agent] [studio] Sprawdzić w emulatorze przycisk „Wstrzyknij awarię” w faceplate (wyłączony dla operatora, alarm P… pojawia się, Kasuj po naprawie) i metodę `InjectFault` w SimulationLogic
  (2026-10-05: bez logowania przycisk wyszarzony — OK; reszta wymaga zalogowania jako serwis)
- [ ] [S] [agent] `Sim.Cli --inject L1/LAB@600`: scenariusz demo z awarią wstrzykniętą w zadanym czasie (przebieg nadal deterministyczny), żeby podgląd pokazał awarię testową
- [ ] [S] [agent] Podgląd/Optix: wskaźnik mikroprzestoju na kaflu stacji (np. „Praca · mikroprzestój” + licznik), bo stan zostaje „Praca”
- [ ] [M] [agent] Osobny strumień RNG także dla `FaultModel` każdego urządzenia — dziś każda zmiana czasu pracy przetasowuje awarie przy tym samym seedzie (KPI demo skaczą o ±15 pp)
- [ ] [S] [agent] Pareto strat OEE: podział słupka na Dostępność / Wydajność / Jakość (dane A/P/Q już w trace-summary) i podpowiedź przyczyny

- [ ] [S] [agent] [studio] Po Build sprawdzić w AlarmGrid: kolumna priorytetu pokazuje 4 różne wartości (PAL Urgent, FILL/CAP High, taśmy/PACK/VIS Medium, FEED/LAB Low)
- [ ] [S] [agent] Wspólne źródło pasm Severity: `AlarmPriority` + `Severity` w layout.json, żeby podgląd (`Alarms`, P1–P4) brał priorytet z Core, nie z własnej tabeli

## Blocked
- [ ] [S] [help 4.5] Pokazy > 2 h (emulator zawsze kończy po 2 h): eksport + runtime jako usługa (blocked: decyzja Maćka o licencji)

## Done
- [x] 2026-10-07 [help 1.2] `stopRequest`, `selectedStation` jako zmienne ekranu linii (per sesja), przyciski przez względne linki;
  usunięte z LineType i CommandBits. [help 1.3] klik w Rectangle przez MouseUp + `HitTestVisible` (`Ui.OnClickSet`).
  [help 1.5] `StepClock`: kroki wg zmierzonego czasu, nie wywołań PeriodicTask; 2 testy. Do sprawdzenia w emulatorze (Bramka A)
- [x] 2026-10-07 [help 1.1] Pasma Severity: P1–P4 → 900/700/400/200 (było 300/500 — oba „Medium” w Optix); `AlarmPriorities.OptixBand`
  (≤250 Low, ≤500 Medium, ≤750 High, >750 Urgent); test: każdy priorytet w swoim paśmie, linia demo używa 4 pasm. W Studio: Build (nowe Severity)
- [x] 2026-10-06 Straty OEE per stacja (Pareto): `OeeLosses` w Core przypisuje czas planowany stacji OEE sprawcy (awaria/obsługa,
  brak podaży w górę, blokada w dół, mikroprzestój, odrzut wg `DefectBy`); OEE + Σ = 100 %; `oeeLosses` w trace-summary,
  sygnał `OeeLossS` na urządzeniu, panel Pareto w `Line1`; 3 testy
- [x] 2026-10-05 [studio] Wdrożenie 42a60b7 w Studio (Build, emulator). Klik „Ekran linii” na hali nic nie robił: `VariableToModify`
  wskazywał zmienną `openTab` typu ekranu, nie instancji w sesji → względny DynamicLink `…/openTab@NodeId` (`Ui.OnClickSet relative`); sprawdzone w web
- [x] 2026-10-05 Mikroprzestoje < 30 s: `MicroStopModel` (własny RNG na urządzenie), `microStop` w factory.json (FILL, CAP),
  stan „Praca” bez alarmu, sygnały `MicroStopActive`/`MicroStops`/`MicroStopS`, liczniki w trace-summary; test bez awarii: Performance −3,6 pp
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
