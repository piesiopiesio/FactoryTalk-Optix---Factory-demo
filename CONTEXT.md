# CONTEXT — Optix Demo Factory

Mapa projektu dla agenta. Czytaj w całości na starcie; resztę kodu tylko gdy zadanie jej dotyczy.
Limit: 200 linii. Indeks plików na dole generuje `python3 tools/ctx.py --write`.

## Misja
Aplikacja demo FactoryTalk Optix: cała fabryka w jednej hali, rozwijana etapami.
Etap 1: linia pakująca butelki (7 stacji + 6 taśm). Dane z symulacji w C#.
Podgląd ekranów w Claude Design odtwarza przebieg z tej samej symulacji.

## Przepływ danych
```
factory.json ──FactoryLoader──▶ Hall (Core) ──SimEngine.Tick(0.1s)──┬─▶ OptixBinder ─▶ Model/Factory/* (Optix runtime)
      │                                                             └─▶ Sim.Cli ─▶ design/data/{trace,layout,trace-summary}.json
      └─FactoryBuilder (Optix design-time) ─▶ typy, instancje, alarmy, ekrany      └─▶ tools/gen_preview.py ─▶ canvas data/preview.json
```

## Warstwy i zasady (niezmienniki)
1. `src/Factory.Core` = czysty C# (C# 10, .NET 8 BCL, zero NuGet). NIGDY `using FTOptix.*` w Core.
2. `src/Factory.Optix` = jedyne miejsce z API Optix. Kompiluje się w chmurze na `stubs/Optix.Stubs`,
   a w Studio jako kopia z `tools/export_optix.py` (docs/studio-setup.md). Stuby = tylko członki sprawdzone na Optix 1.7.
3. Właściwość z `[Signal]` = zmienna Optix (camelCase) + kolumna w trace + wiersz w faceplate. Bez dodatkowego kodu.
4. Podgląd nie ma logiki symulacji: artboardy tylko odtwarzają `data/preview.json`.
5. Generowane (nie edytuj ręcznie): Optix `Model/Factory`, `Model/Templates/Factory`, `Alarms/Factory`,
   `UI/Screens/HallScreen`, `UI/Screens/LineScreen_*`, `UI/MainWindow/{Background,Header,MainNav}`, `NetSolution/Factory/**`;
   repo `design/canvas/project/data/*`, `design/data/*`, `dist/`.
6. Pliki Studio (`optix/**/*.yaml`) należą do Studio. Agent ich nie edytuje.
7. Jedna klasa = jeden plik ≤ 250 linii, pierwsza linia `// @summary: …`.
8. Determinizm: seed z `factory.json`; te same wejścia = ten sam przebieg (test `SameSeedSameRun`).

## Model domeny (src/Factory.Core)
- `Equipment` (abstr.): Id, Name, State, FaultModel (MTBF/MTTR), komendy Start/Stop/Reset, czas w stanach,
  `AlarmPriority` 1–4 (→ Severity 900/700/400/200, po jednym w każdym paśmie Optix; `AlarmPriorities.OptixBand`).
- `Station : Equipment`: cykl acquire → `Process(item)` → emit; Starved/Blocked; liczniki Processed/Good/Reject.
- Stacje (`Stations/`, klucz `[StationType]`): feeder, filler, capper, labeler, vision, casepacker, palletizer.
- `Conveyor : Equipment`: akumulująca taśma (pitch, length, speed), blokuje stację przed sobą.
- `Line`: stacje w łańcuchu + taśmy; KPI: OEE = A×P×Q (na `oeeStation`), ThroughputPerMin (60 s), Good/Reject.
- Przestoje planowe (`Maintenance` przez `Station.Hold`): wymiana palety (PAL), wymiana rolki etykiet (LAB),
  uzupełnianie zbiornika FILL (`LowTank` → refill).
- Mikroprzestoje (`MicroStopModel`, `microStop` w factory.json, < 30 s): stan „Praca” bez alarmu, brak postępu = strata Wydajności;
  własny RNG na urządzenie.
- `OeeLosses` (w `Line`): Pareto strat — czas planowany stacji OEE przypisany sprawcy (awaria/obsługa, brak podaży → w górę,
  blokada → w dół do pierwszego niestojącego w kolejce, mikroprzestój, odrzut wg `Item.DefectBy`); OEE + Σ = 100 %; sygnał `OeeLossS`.
- `Hall`: strefy (tylko layout) + linie. `SimEngine`: Tick, Command("L1" | "L1/FILL" | "Hall"; `InjectFault` tylko urządzenie), Events, Nodes().
- `Layout`: geometria ekranów z manifestu (canvas 1600×900; Optix ×1.2) + lint.
- Stany: Stopped 0, Running 1, Starved 2, Blocked 3, Faulted 4, Maintenance 5; kolory `StatePalette` = `design/theme.json`.

## Jak dodać stację (3 kroki)
1. `src/Factory.Core/Stations/Xyz.cs`: `[StationType("xyz")] class Xyz : Station`, nadpisz `Process`, dodaj `[Signal]`, `FaultCatalog`.
2. `factory.json`: wpis w `stations` (id, type, name, pos [x,y], cycleS, fault, params) + taśmy `from`/`to`.
3. `make check`. FactoryBuilder i podgląd podchwycą stację automatycznie.

## Optix (ścieżki w projekcie `Factory_demo`, Studio 1.7.5.13)
- `Model/Factory/{Hall|L1|L1/FILL}/<signal>` + `stateColor` (UInt32 ARGB). `[Signal(Label=…)]` = podpis na ekranie.
  Typy: `Model/Templates/Factory/<Klasa>Type` (sufiks obowiązkowy — proxy Studio przesłania klasy Core).
- Polecenia HMI = bity `cmdStart/cmdStop/cmdReset` (linia), `cmdReset`/`cmdFault` (urządzenie; awaria testowa), `Hall/timeScale`; przyciski
  `VariableCommands.Set`, wykonuje `CommandBits` w runtime NetLogic `Model/SimulationLogic` (tworzy go Build).
- NetLogic design-time `NetLogic/FactoryBuilder`: `Build()` generuje model, alarmy, ekrany i zakładki; `Clean()`.
  Build uruchamia się w Studio (prawy klik → Execute), NIE przez most ftx-mcp (wątek HTTP może zamknąć Studio).
- Widoki: `HallView` (poziom 1 + mini-mapa; kafel → zakładka linii przez `HallScreen/openTab` + `AccessLogic`, kolejność zakładek = Core `NavTabs`), `LineView` (schemat), `StationTable`, `LinePanel` (KPI, komendy, awarie),
  `WindowGenerator` (nagłówek + NavigationPanel), `LibraryViews` (Alarmy, Trendy z Template Library), `LoggerGenerator`
  (lista z Core `TrendPens`: kolumna bazy = BrowseName — nie zmieniać formatu, nazwa pióra = DisplayName).
  Liczby formatuje StringFormatter wg locale sesji (konta demo `pl-PL`, Anonymous = Locales projektu). Widgety tylko przez `Ui.cs`. Elementy Template Library (styl ISA, AlarmBanner/Grid, AdvancedTrend, loggery) dodaje się
  raz w Studio — lista w docs/studio-setup.md; brak = fragment pominięty z ostrzeżeniem.
- Pętla ze Studio (sesja połączona z komputerem): `export_optix.py` → wgranie plików → `optix_build_check` →
  Execute Build → `optix_emulator restart` → `optix_observe screenshot`.

## Styl HMI (skrót; pełne zasady: docs/hmi-style.md)
Rockwell Process HMI Style Guide / ISA-101: tło `#E0E0E0`, obrys urządzeń `#A0A0A4`, wnętrze = tło, dane `#475CA7`,
Arial. Stany: Stop `#808080`, Praca `#F0F0F0`, oczekiwanie/obsługa `#93C2E4` + tekst. Kolory alarmowe
(`#E22028`, `#EC8629`, `#F5E11B`, `#916AAD`) WYŁĄCZNIE dla alarmów. Ekrany: poziom 1 hala, 2 linia, 3 stacja, 4 diagnostyka.
Przyciski ≥ 40 px, odstęp 10 px. Stan nigdy tylko kolorem.

## Podgląd (Claude Design)
Canvas: https://claude.ai/artifact/5UbGvx3SZaw2ncuJ9pPdrE — artboardy `Main` (hala), `Line1`, `Alarms` (zdarzenia z czasem trwania, pod `Line1`), `Status`.
Pliki w `design/canvas/project/`; publikacja: root = `design/canvas`, tylko zmienione pliki.
Kopia repo (gdy brak GitHuba): `project/data/repo.bundle.b64` (`make bundle`).

## Komendy
- `make check` — build (Core, Optix na stubach, Cli, testy) + testy + symulacja 30 min + preview + lint + limity kontekstu.
- `make snapshot` — PNG hali i linii w `design/snapshots/` (obejrzyj przed commitem).
- `make sim MIN=60` — dłuższy przebieg; wynik w `design/data/trace-summary.json` (czytaj ten plik, nie trace.json).
- Testy: `dotnet run --project tests/Factory.Tests [filtr]`.

## Pliki robocze
`BACKLOG.md` (kolejka), `JOURNAL.md` (14 dni), `AGENT_DAILY.md` (procedura dnia), `docs/studio/feedback.md` (uwagi ze Studio — priorytet),
`docs/hmi-style.md` (styl), `docs/studio-setup.md` (konfiguracja Studio), `tools/sync_remote.sh` (GitHub ↔ kopia).

## Indeks plików
<!-- index:start -->
- `design/canvas/project/Alarms.dc.html` — artboard: Alarmy i zdarzenia linii 1 — poziom 2
- `design/canvas/project/Line1.dc.html` — artboard: Linia pakująca 1 — poziom 2
- `design/canvas/project/Main.dc.html` — artboard: Hala — przegląd, poziom 1
- `design/canvas/project/Status.dc.html` — artboard: Stan projektu — poziom 4
- `src/Factory.Core/Factory.Core.csproj` — Pure C# domain + simulation. No FTOptix references allowed.
- `src/Factory.Core/Manifest/FactoryLoader.cs` — Parses + validates factory.json and builds the Hall object graph (stations, conveyors, links).
- `src/Factory.Core/Manifest/FactoryManifest.cs` — DTOs mirroring factory.json (camelCase JSON). Pure data, no behavior.
- `src/Factory.Core/Manifest/Layout.cs` — Screen geometry from factory.json (1600x900 logical canvas): station/conveyor rects + layout lint. Used by Optix builder and preview.
- `src/Factory.Core/Manifest/NavTabs.cs` — Order of the main navigation tabs (Hala, one per line, Alarmy, Trendy, Logowanie) and the tab index of each line screen.
- `src/Factory.Core/Manifest/StationRegistry.cs` — Maps manifest "type" keys to Station classes via [StationType]; the single extension point for new stations.
- `src/Factory.Core/Model/AlarmPriority.cs` — Alarm priority 1-4 (ISA-18.2 / Rockwell HMI guide) per equipment type and its OPC UA Severity (1-1000) mapping.
- `src/Factory.Core/Model/Conveyor.cs` — Accumulating belt: items keep pitch spacing, travel length/speed, block the upstream station when full.
- `src/Factory.Core/Model/Equipment.cs` — Base of every simulated device: identity, commands, fault model, state machine hook, time-in-state.
- `src/Factory.Core/Model/Hall.cs` — Root of the factory: hall metadata, zones (layout only) and production lines.
- `src/Factory.Core/Model/Item.cs` — A product unit flowing through the line (bottle, case or pallet) with quality flags.
- `src/Factory.Core/Model/Line.cs` — Production line: ordered stations + conveyors, line commands, aggregate state and OEE/throughput KPIs.
- `src/Factory.Core/Model/MachineState.cs` — Machine states + shared color palette (used by Optix binder and Design preview).
- `src/Factory.Core/Model/OeeLosses.cs` — OEE loss Pareto: every lost second at the OEE station blamed on the equipment that caused it (fault, starved/blocked root, micro stop, reject origin).
- `src/Factory.Core/Model/SignalAttribute.cs` — [Signal] marks a property exported to Optix variables, trace.json and the preview.
- `src/Factory.Core/Model/Station.cs` — Template-method work cycle for stations: acquire -> process (subclass) -> emit; tracks starved/blocked.
- `src/Factory.Core/Model/TrendPens.cs` — Signals worth trending (line KPIs, station doubles, belt occupancy) with DB column names and readable pen labels.
- `src/Factory.Core/Sim/FaultModel.cs` — Random failures: exponential MTBF on running time, MTTR repair, optional auto-recover.
- `src/Factory.Core/Sim/MicroStopModel.cs` — Short stops (< 30 s) while running: exponential gap on running time, short pause; a Performance loss, not a fault or alarm.
- `src/Factory.Core/Sim/SimContext.cs` — Per-tick context passed to every node: dt, clock, RNG, event sink, id generator.
- `src/Factory.Core/Sim/SimEngine.cs` — Runs the hall: fixed-step Tick(dt), commands by path ("L1", "L1/FILL"), event log, node enumeration.
- `src/Factory.Core/Sim/SimEvent.cs` — Alarm/event record (fault raised/cleared, maintenance, low tank) emitted by the engine.
- `src/Factory.Core/Sim/SimRandom.cs` — Seeded RNG (deterministic runs) with normal, exponential and chance helpers.
- `src/Factory.Core/Sim/StepClock.cs` — Wall-clock pacing for the fixed-step engine: steps per call follow measured elapsed time (PeriodicTask period + run time).
- `src/Factory.Core/Stations/Capper.cs` — Screws caps; torque outside tolerance marks the bottle defective.
- `src/Factory.Core/Stations/CasePacker.cs` — Collects bottles into cases (unitsPerCase) and emits one case item per full case.
- `src/Factory.Core/Stations/Feeder.cs` — Source station: creates empty bottles at its cycle rate (infinite supply).
- `src/Factory.Core/Stations/Filler.cs` — Fills bottles from a buffer tank refilled in batches (LowTank -> Maintenance); noisy fill volume marks defects.
- `src/Factory.Core/Stations/Labeler.cs` — Applies labels from a roll; random misses mark the bottle defective; an empty roll triggers a timed roll change (Maintenance).
- `src/Factory.Core/Stations/Palletizer.cs` — Line sink: stacks cases on pallets; a full pallet triggers a timed pallet change (Maintenance).
- `src/Factory.Core/Stations/VisionInspector.cs` — Inspects every bottle; anything not filled, capped, labeled and defect-free is rejected.
- `src/Factory.Optix/AccessLevels.cs` — Operator access model: level from the session user's groups (0 anonymous, 1 Operatorzy, 2 UtrzymanieRuchu) + control thresholds.
- `src/Factory.Optix/AccessLogic.cs` — Runtime UI NetLogic in MainWindow: enables commands/resets by the session user's level; hall tile click switches the tab.
- `src/Factory.Optix/AlarmGenerator.cs` — One DigitalAlarm per equipment in Alarms/Factory, linked to Model/Factory/<path>/faultActive; Severity from priority 1-4; operator acknowledges.
- `src/Factory.Optix/CommandBits.cs` — Runtime side of HMI commands: polls cmdStart/cmdStop/cmdReset/cmdFault bits and Hall/timeScale, executes on SimEngine, clears bits.
- `src/Factory.Optix/Factory.Optix.csproj` — Cloud compile check of the NetLogic layer against stubs. In Studio these .cs files are linked into NetSolution instead.
- `src/Factory.Optix/FactoryBuilder.cs` — Design-time NetLogic: Build() regenerates types, Model/Factory, alarms, screens, tabs from factory.json + demo accounts; Clean() removes them.
- `src/Factory.Optix/HallView.cs` — Level 1 screen (HallScreen): zones, one tile per line with state, KPIs and a live mini-map of its stations.
- `src/Factory.Optix/LibraryViews.cs` — Screens built from Template Library widgets: Alarms (AlarmGrid + history), Trends (AdvancedTrend), Login (LoginForm).
- `src/Factory.Optix/LinePanel.cs` — Right-hand panel of a line screen: state, OEE/KPIs, commands (Start, Stop with confirmation, reset, time) by user role, active fault list.
- `src/Factory.Optix/LineView.cs` — Level 2 line diagram (LineScreen_<id>): belts with occupancy bars and station tiles (state, counts, own signals, progress).
- `src/Factory.Optix/LoggerGenerator.cs` — Fills Loggers/DataLogger1 with Core's TrendPens: column = BrowseName (stable), readable pen name = DisplayName.
- `src/Factory.Optix/ManifestSource.cs` — Loads ProjectFiles/factory.json inside Optix (builds the Core Hall); design-time sync copies the repo-root manifest in.
- `src/Factory.Optix/ModelGenerator.cs` — Builds Model/Factory (Hall + lines with stations and conveyors, paths match SimEngine.Nodes()) and ensures Model/SimulationLogic.
- `src/Factory.Optix/NodeUtil.cs` — Small node helpers for generators: ensure/reset folders, clear children, delete by name.
- `src/Factory.Optix/OptixBinder.cs` — Runtime bridge: writes changed [Signal] values (+ stateColor) from SimEngine into Model/Factory variables.
- `src/Factory.Optix/OptixNames.cs` — Optix-side naming: project paths, camelCase variable names, .NET -> OPC UA data type mapping, UI colors, screen scale.
- `src/Factory.Optix/ScreenGenerator.cs` — Regenerates the HMI from factory.json: HallScreen, LineScreen_<id> per line, Alarms/Trends/Login screens, MainWindow chrome/tabs.
- `src/Factory.Optix/SecurityGenerator.cs` — Demo accounts: design time creates groups Operatorzy/UtrzymanieRuchu + users (locale pl-PL) from demo-users.json; runtime sets their test passwords.
- `src/Factory.Optix/SimulationLogic.cs` — Runtime NetLogic (Model/SimulationLogic): ticks SimEngine every 100 ms, publishes signals, executes HMI command bits, InjectFault method, sets demo passwords.
- `src/Factory.Optix/StationDetail.cs` — Level 3 station faceplate on the line screen: tile click opens it; Template Library graphic moved only by live signals.
- `src/Factory.Optix/StationTable.cs` — Station table under the line diagram: one row per station (state, good/reject, cycle, speed, progress, fault message).
- `src/Factory.Optix/TypeGenerator.cs` — Creates one Optix ObjectType per Core class (FillerType, ConveyorType, LineType, HallType...) with a variable per [Signal].
- `src/Factory.Optix/Ui.cs` — Widget factory for generated screens (pixels): boxes, labels, buttons, bindings, formatters, click -> set variable.
- `src/Factory.Optix/WindowGenerator.cs` — MainWindow chrome owned by the builder: background, header (plant KPIs + fault annunciator) and NavigationPanel tabs.
- `src/Sim.Cli/Checks.cs` — Behavior checks on a finished run: KPI targets, faults present, no dead station, bottle conservation.
- `src/Sim.Cli/LayoutExport.cs` — Writes design/data/layout.json (zones, line rects, station + belt rects) so the preview never re-implements geometry.
- `src/Sim.Cli/Program.cs` — CLI entry: `Sim.Cli [--manifest factory.json] [--minutes 30] [--frame 2] [--out design/data]`; exit 1 if a check fails.
- `src/Sim.Cli/Sim.Cli.csproj` — Console runner: simulates the hall, writes design/data/trace.json + trace-summary.json, checks KPI targets.
- `src/Sim.Cli/TraceRecorder.cs` — Samples all [Signal] values + belt positions every frame into a compact columnar trace.
- `stubs/Optix.Stubs/FTOptix.cs` — Stubs for FTOptix.* namespaces (NetLogic, Project, InformationModel, UI widgets, converters, events, alarms) - verified against Optix 1.7 build.
- `stubs/Optix.Stubs/UAManagedCore.cs` — Stubs for UAManagedCore (nodes, variables, values, NodeId, Log) - only members our code uses; shapes verified against Optix 1.7.
- `tests/Factory.Tests/CoreTests.cs` — Tests for manifest validation, conveyor/flow behavior, determinism, KPIs, signals and palette.
- `tests/Factory.Tests/FaultTests.cs` — Tests for injected (operator/test) faults: device-only, event pair, repair time, no effect on the RNG stream.
- `tests/Factory.Tests/Fixtures.cs` — Test helpers: repo paths, loading the real factory.json, building small ad-hoc lines.
- `tests/Factory.Tests/MicroStopTests.cs` — Tests for micro stops (< 30 s): no alarm/state change, Performance loss of 2-5 pp on the real line, validation.
- `tests/Factory.Tests/OeeLossTests.cs` — Tests for the OEE loss Pareto: losses + OEE = 100 %, starved/blocked blamed on the stopped neighbour, rejects on the defect's origin.
- `tests/Factory.Tests/StepClockTests.cs` — Tests for StepClock: simulated time follows wall time despite late PeriodicTask calls; stalls are dropped.
- `tests/Factory.Tests/TestRunner.cs` — Minimal test harness: discovers static methods marked [Test], runs them, prints PASS/FAIL, exit code.
- `tools/check_design.py` — Static lint of the Design canvas: canvas.json <-> artboards, required head line, hole syntax, sizes, data files.
- `tools/ctx.py` — Context budget guard + file index: `--check` enforces token/line limits, `--write` refreshes the index in CONTEXT.md.
- `tools/export_optix.py` — Plan B deploy: copies src/Factory.Core + src/Factory.Optix + factory.json into an Optix project layout (dist/optix).
- `tools/gen_preview.py` — Packs design/data/{layout,trace}.json into design/canvas/project/data/preview.json (columnar, compact) for the Design canvas.
- `tools/gen_status.py` — Builds design/canvas/project/data/status.json (checks, tests, KPIs, backlog, journal, context) for the Status artboard.
- `tools/snapshot.py` — Renders PNG snapshots (hall + line) from preview.json via headless Chromium so the agent can look at its own UI.
- `tools/sync_remote.sh` — Connects the local repo to GitHub when this session can reach it; merges remote state into claude/dev. Prints REMOTE=github|none.
<!-- index:end -->
