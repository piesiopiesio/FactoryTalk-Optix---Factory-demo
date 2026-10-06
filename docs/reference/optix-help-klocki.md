# Klocki z pomocy Optix 1.7.5.13 — notatki do wdrożenia

Plan i kolejność: dokument „Optix Demo Factory — plan”, zakładka „Klocki z pomocy Optix”.
Źródło: pomoc Studio przekonwertowana do Markdown na PC Maćka (`Documents\Rockwell Automation\FactoryTalk Optix\Pomoc-md`,
tylko do użytku własnego — nie kopiować treści do repo). Poniżej własne streszczenia: nazwy API, właściwości i pułapki.
Pozycje backlogu `[help X.Y]` odsyłają tutaj. Wszystko, co nie było jeszcze widziane w emulatorze, oznaczone (?).

## Etap 1 — poprawki
- **1.1 Severity.** Pasma Optix: 1–250 Low, 251–500 Medium, 501–750 High, 751–1000 Urgent (Alarms › Alarm details).
  Nasze 300/500/700/900 dają P3 i P4 jako „Medium” → 200/400/700/900. `AlarmController.Severity` (UInt16) już używane.
- **1.2 Zmienne per sesja.** Każdy klient web = osobna sesja i osobna instancja okna; zmienne w Model są wspólne dla wszystkich.
  `selectedStation`, `stopRequest` przenieść do ekranu linii (jak `openTab` w HallScreen), przyciski: `Ui.OnClickSet(..., relative: true)`,
  `Visible` przez link do zmiennej ekranu (SetDynamicLink z węzła w tym samym typie liczy ścieżkę względną).
- **1.3 Klik w Rectangle.** Rectangle/Panel/Image mają tylko MouseDown i MouseUp (MouseClick mają Button, CheckBox…), a
  `HitTestVisible` domyślnie false (klik przechodzi pod spód). Cel kliknięcia: `MouseUpEvent` + `HitTestVisible = true`.
- **1.4 Locale.** Sesja bez logowania bierze pierwsze locale z właściwości projektu Locales; UISession ma `LocaleId` (?) i listę
  locale zapasowych (gdy brak tekstu w danym języku). Legenda AdvancedTrend = `TrendPen.Title` (LocalizedText) — ustawiać tytuł
  pióra wprost, w en-US i pl-PL (`InformationModel` translations / LocalizedText z locale). Dziś DisplayName ma tylko pl-PL.
- **1.5 Takt.** Rzeczywisty okres PeriodicTask = okres + czas wykonania kodu (przykład z pomocy: 1000 ms + 500 ms = 1500 ms).
  Mierzyć upływ (Stopwatch) i wykonywać tyle kroków stałego `Dt`, ile minęło (z limitem nadrabiania), żeby zachować determinizm.

## Etap 2 — funkcje wbudowane
- **2.1 Bezczynność.** UISession: `IdleTimeoutEnabled`, `IdleTimeoutDuration` (Duration), zdarzenie `IdleTimeoutEvent` → metoda
  Logout. UISession musi być przypisana do obu presentation engine. Zastępuje IdleTimeoutLogic z Template Library.
- **2.2 Nawigacja.** NavigationPanel: `CurrentTabIndex`, metoda `ChangePanelByTabIndex(Index, AliasNode)`; PanelLoader: `ChangePanel`.
  Klik kafla hali wywołuje metodę MainNav zamiast pisać `openTab` (ObjectPointer do MainNav w tej samej sesji — link względny (?)).
- **2.3 Uprawnienia.** `Enabled` powiązane ze zmiennymi grup/ról sesji (`{Session}` …/Groups/<grupa>), dla OR — ExpressionEvaluator.
  Pomoc podaje 0 = członek, 1 = nie (?) — sprawdzić w emulatorze przed usunięciem pętli z AccessLogic.
- **2.4 Dialog z aliasem.** Typ Dialog (`Modal`, zamykanie), w nim zmienna-alias z `Kind` = typ bazowy stacji; widgety linkują
  `{Station}/<sygnał>`. Przycisk: `UICommands.OpenDialog(Dialog, AliasNode = Model/Factory/L1/FILL)`. Wymaga typu bazowego stacji,
  po którym dziedziczą typy klas (TypeGenerator). Tworzenie z C# i linki aliasowe w 1.7 (?) — 1.8 ma `SetDynamicLinkToAlias`.
- **2.5 Ikony i miganie.** Obiekt System (Model › New › System) daje zmienne Blink fast/medium/slow. MultistateImage P1–P4,
  `Blink` dopóki alarm niepotwierdzony (`AckedState/Id`). LED ma „Blink when active”. Kolory w runtime tylko hex.
- **2.6 Sparkline i progi.** `Sparkline.PenValue` (link), `TimeWindow`, pasmo `RangeLow/RangeHigh/RangeColor` (szare, ISA-101).
  `LinearGauge` + `WarningZones` (From/To/Color) dla zbiornika. `TrendThreshold` (Value/Color/Thickness) — cel OEE na trendzie.
  Szybko zmieniane Min/Max gauge spowalniają runtime.
- **2.7 Węzły.** Koszt orientacyjny: link dynamiczny ~3 węzły, klik z Set ~24, konwertery więcej. Ukryte obiekty i tak ładują się
  z ekranem. Etykieta `{0}` bez StringFormatter = zwykły link. Pomiar: Nodes Counter (Best practices › Performance metrics).

## Etap 3 — analityka
- **3.1 Pareto.** Dane `oeeLossS` są od 06.10. HistogramChart: `Model` = Model/Factory/L1, `Label` = BrowseName/DisplayName,
  `Value` = sygnał straty. Sortowanie nieopisane (?) — w razie potrzeby sortować w NetLogic do osobnego folderu.
- **3.2 Historia alarmów.** SQL na tabeli loggera alarmów: GROUP BY, COUNT/SUM, ORDER BY, LIMIT działają; brak CASE/WHEN, agregatów
  w WHERE, LAG. Czasy trwania: parować aktywny→nieaktywny w runtime NetLogic (`Store.Query`, wynik do tabeli lub zmiennych).
  Liczby w SQL zawsze w InvariantCulture. Pomoc niespójna co do DISTINCT/UNION (?).
- **3.3 Raport PDF.** Obiekt Report: Header, Sections (`PanelSection`, `DataGridSection` z modelem lub zapytaniem), Footer.
  `GeneratePdf(OutputPath, LocaleId = "pl-PL")`, wynik w `GeneratePdfCompletedEvent` (0 OK). Jednostki raportu zależą od locale projektu.
- **3.4 Receptury.** RecipeSchema z `TargetNode` = obiekt parametrów SKU; widget RecipeEditor; zastosowanie
  `TransferFromStoreToTarget(RecipeId, TargetNodeId, ErrorPolicy)`. Receptury NIE mogą dzielić bazy z DataLoggerem/EventLoggerem →
  osobna EmbeddedDatabase.

## Etap 4 — łączność i pokaz
- **4.1 OPC UA.** Jeden `OPCUAServer` na projekt; `EndpointURL` domyślnie opc.tcp://localhost:59100; `NodesToPublish` → wpis z
  `Nodes` i `Users` (min. jeden, może Anonymous); pusta lista = cały projekt. Demo: `MinimumSecurityPolicy`/`MessageSecurityMode` = None.
  `UseNodePathInNodeIds = true` (Build tworzy węzły od nowa, NodeId by się zmieniały). Metody NetLogic widoczne jako metody OPC UA.
  Pełna publikacja blokuje „Transfer optimized project”.
- **4.2 MQTT.** Wbudowany broker (do 25 połączeń, MQTT Users), `MQTTClient` (`BrokerAddress`, `BrokerPort`, `Status`),
  `MQTTPublisher` z `DataSource` = DataLogger1 lub folder globalny (tylko zmienne skalarne), JSON stały lub własny (`PF*`). Brak Sparkplug.
- **4.3 Weihenstephan.** Companion specifications w Type view (WS, PackML); tworzone tylko obowiązkowe dzieci, opcjonalne przez
  New › Optional objects. Rozpoznanie: co da się zbudować z C#.
- **4.4 Import Template Library.** `TemplateLibrary.ImportLibraryItem(dest, nodeClass, library, itemPath, conflicts, preservePaths)`;
  tylko z włączonym „Show feature preview”, tylko szablony lokalne; przed importem commit.
- **4.5 Pokazy.** Emulator zatrzymuje się po 2 h nawet z licencją. Dłużej: eksport aplikacji (Windows x64) i FTOptixRuntime albo
  usługa Windows (tylko web, port 49100 do wdrożeń). Bez licencji na celu też 2 h. Docker (Ubuntu 22.04) z kluczem licencji.

## Ogólne pułapki z pomocy
- Linki tworzone w NetLogic są domyślnie Read; pola edycyjne wymagają `DynamicLinkMode.ReadWrite`.
- Kod async nie może czytać ani pisać modelu — tylko PeriodicTask/DelayedTask/LongRunningTask; `Dispose` czeka na koniec kodu.
- Każde `+=` na zdarzeniu wymaga `-=` w `Stop()`.
- NetLogic w typie UI startuje osobno w każdej sesji i presentation engine.
- Po Build: przycisk „Find dynamic links” pokazuje zerwane linki.
