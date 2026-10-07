# Konfiguracja w FactoryTalk Optix Studio

Projekt Maćka: `Factory_demo`, Optix Studio 1.7.5.13 (NetSolution net8.0), folder
`Documents\Rockwell Automation\FactoryTalk Optix\Projects\Factory_demo` (lokalny git Studio, bez remote).

## Jak kod trafia do Studio (Plan B — obowiązujący)
Repo nie jest sklonowane na komputerze Maćka, więc kod kopiujemy:

1. `python3 tools/export_optix.py` → `dist/optix/` w układzie projektu Optix:
   - `ProjectFiles/NetSolution/Factory/Core/**` (czysty C#) i `.../Factory/Optix/*` (NetLogic),
   - `ProjectFiles/factory.json`.
2. Sesja Claude połączona z komputerem wgrywa te pliki do folderu projektu (narzędzie commit plików).
   NetSolution to projekt SDK: kompiluje każdy `.cs` w folderze — `.csproj` się nie edytuje.
3. `optix_build_check` (ftx-mcp) kompiluje kopię NetSolution na prawdziwych bibliotekach Optix 1.7.
   Stuby w `stubs/Optix.Stubs` odwzorowują tylko sprawdzone tam API.

Pliki w `NetSolution/Factory` są generowane — edytuj `src/` w repo.

## Węzły w projekcie
- `NetLogic/FactoryBuilder` — design-time NetLogic. Utworzony w Studio (prawy klik NetLogic → New → Design-time NetLogic
  → Rename), bo tylko wtedy Studio dopisuje węzły metod (menu Execute). Klasa: `NetSolution/FactoryBuilder.cs`.
  **Build** uruchamia się w Studio: prawy klik → Execute Build (wątek UI Studio; wywołanie przez most ftx-mcp
  z wątku HTTP może zamknąć Studio).
- `Model/SimulationLogic` — runtime NetLogic, tworzy go Build, jeśli brakuje. Klasa: `NetSolution/SimulationLogic.cs`.
- `NetLogic/FactoryBuilder` → Execute **CreateDemoUsers** — grupy `Operatorzy`, `UtrzymanieRuchu` i konta z
  `Factory_demo/demo-users.json` (katalog projektu, poza repo i ProjectFiles; hasła testowe w pamięci projektu Claude).
- `UI/MainWindow/AccessLogic` — runtime NetLogic (uprawnienia, przełączanie zakładki z hali), tworzony przez Build raz i nigdy
  nie kasowany. Klasa: `NetSolution/AccessLogic.cs`.

## Pułapki (sprawdzone 2026-09-25)
- Klasa NetLogic musi leżeć w `NetSolution/<NazwaWęzła>.cs`; inaczej Studio dopisze tam szablon → duplikat klasy.
- Typy w projekcie mają sufiks `Type` (`LineType`, `FillerType`…): Studio generuje globalne klasy proxy o nazwie typu,
  a proxy `Line`/`Hall` przesłoniłoby klasy Core i zepsuło kompilację.
- Przyciski NIE wołają metod NetLogic (węzeł utworzony z kodu nie ma węzłów metod). Ustawiają bity poleceń
  (`cmdStart`/`cmdStop`/`cmdReset`, `Hall/timeScale`) wbudowanym `VariableCommands.Set`; `SimulationLogic`
  wykonuje je co 100 ms i kasuje (handshake jak w PLC).
- Ctrl+S w Studio uruchamia kompilację NetSolution (po wgraniu plików). Execute działa na ostatniej udanej kompilacji.
- Nowa metoda [ExportMethod] trafia do menu Execute dopiero po Ctrl+S w Studio; sama zmiana pliku (auto-kompilacja) nie dodaje węzła metody.
- `Session.ChangePassword` nie działa w Studio (brak obsługi metody) — hasła kont ustawia runtime.
- Czarne zrzuty ekranu (widać tylko pasek zadań) po starcie PC: Win+Ctrl+Shift+B (restart sterownika grafiki).
- NetLogic tworzony przez Build (usuwany i tworzony od nowa) dostaje od Studio pusty szablon klasy w miejsce naszego pliku →
  węzły NetLogic tworzyć raz i nie kasować (AccessLogic siedzi w MainWindow, poza `WindowParts`).
- Rectangle/Panel/Image nie mają zdarzenia MouseClick (tylko MouseDown/MouseUp) i domyślnie `HitTestVisible = false` → cel kliknięcia
  = MouseUp + `HitTestVisible` (`Ui.OnClickSet` robi to sam; pomoc: Events › Objects predefined with events). Przyciski „Szczegóły”/„Ekran linii”
  zostają, dopóki web tego nie potwierdzi.
- Przycisk ustawiający zmienną WŁASNEGO ekranu (np. `HallScreen/openTab`): `VariableToModify` jako względny DynamicLink
  `…/openTab@NodeId` (`Ui.OnClickSet(..., relative: true)`). Sam NodeId wskazuje zmienną typu ekranu, a nie instancji w sesji —
  klik nic nie zmieniał (sprawdzone 2026-10-05).
- Sterowanie ekranem: nie wywoływać „open application” dla działającego Studio (otwiera drugą instancję);
  ikona obok książek („Open .NET Solution”) uruchamia VS Code.

## Elementy dodane raz w Studio (Template Library, 2026-09-25)
Wstawione przeciągnięciem z okna Libraries (ikona książek na pasku) — Build ich nie tworzy, tylko używa:
- `UI/ISAStyleSheet1` (ISA Style Sheet) — styl obu presentation engine (`StyleSheet` w Native i Web).
- `UI/AlarmBanner`, `UI/AlarmGrid`, `UI/AlarmHistoryGridWithFilter` (typy), `UI/AdvancedTrend/*` (folder z `AdvancedTrendMain`).
- `UI/LoginForm/*` (folder z typem `LoginForm`), `UI/UsernameLabel` (typ) — dodane 2026-09-26.
- GraphicElements do faceplate'ów stacji: `UI/Bottle1`, `UI/Carton`, `UI/PalletBoxSide`, `UI/PhotoEyeSensorSide` (2026-09-26).
  Okno Libraries: ikona książek na pasku narzędzi (x≈314 przy 1456×819), wyszukiwarka po nazwie wyświetlanej („Bottle”, „Pallet Box”).
- `DataStores/EmbeddedDatabase1`, `Loggers/AlarmsEventLogger1` (Store = EmbeddedDatabase1),
  `Loggers/DataLogger1` (Store = EmbeddedDatabase1, co 1 s; zmienne dopisuje `LoggerGenerator`).
Brak któregoś elementu = Build pomija ten fragment i pisze ostrzeżenie w Output (nazwy w `OptixNames.Lib*`).

## Format liczb po polsku (locale sesji)
StringFormatter (`"Dobre {0} szt."`) formatuje liczby wg locale sesji: `en-US` daje „1,612”, `pl-PL` — „1 612” i „74,6”.
- Konta demo: **CreateDemoUsers** ustawia każdemu `LocaleId = pl-PL` (`OptixNames.Locale`) — po zalogowaniu format polski.
- Sesja bez logowania (Anonymous) bierze locale projektu: w Studio zaznacz węzeł projektu → właściwość **Locales** →
  `pl-PL` na pierwszym miejscu (jednorazowo, plik Studio — agent go nie zmienia). Do sprawdzenia w emulatorze (pozycja [studio]).

## Co generuje Build (wszystko z `factory.json`)
- `Model/Templates/Factory/*` — typy (stacje, Conveyor, Line, Hall) ze zmiennymi `[Signal]` + `stateColor`.
- `Model/Factory/Hall`, `Model/Factory/L1/<stacja|taśma>` — instancje.
- `Alarms/Factory/*` — DigitalAlarm na `faultActive` każdego urządzenia.
- `UI/Screens/HallScreen` (poziom 1) i `UI/Screens/LineScreen_<id>` (poziom 2: schemat, tabela stacji, panel KPI + komendy + awarie).
- `UI/Screens/AlarmsScreen` (AlarmGrid z potwierdzaniem + historia z filtrem czasu) i `UI/Screens/TrendsScreen` (AdvancedTrend na DataLogger1).
- `UI/Screens/LoginScreen` (LoginForm na `Security/Users` + opis ról); w panelu linii Stop z potwierdzeniem (`stopRequest`).
- `Loggers/DataLogger1/VariablesToLog`: KPI linii, własne sygnały stacji (zbiornik, moment…), zapełnienie taśm.
- W `UI/MainWindow`: `Background`, `Header` (średnie OEE, linie w pracy, AlarmBanner, lampka awarii, użytkownik), `MainNav` (zakładki).
Wszystko inne (np. `UI/Custom`, własne ekrany) Build zostawia w spokoju.

## Weryfikacja
Emulator (F5 / `optix_emulator restart`) → podgląd w Chrome przez CDP (`optix_observe screenshot`).
Emulator: `http://localhost:8081` (Web presentation engine z SetupProject). Log: `optix_emulator action=log`.
