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
   a w Studio przez linki w `.csproj` NetSolution (docs/studio-setup.md). Stuby = tylko używane członki.
3. Właściwość z `[Signal]` = zmienna Optix (camelCase) + kolumna w trace + wiersz w faceplate. Bez dodatkowego kodu.
4. Podgląd nie ma logiki symulacji: artboardy tylko odtwarzają `data/preview.json`.
5. Generowane (nie edytuj ręcznie): Optix `Model/Factory`, `Model/Templates/Factory`, `Alarms/Factory`,
   `UI/Screens/*/FactoryContent`; repo `design/canvas/project/data/*`, `design/data/*`.
6. Pliki Studio (`optix/**/*.yaml`) należą do Studio. Agent ich nie edytuje.
7. Jedna klasa = jeden plik ≤ 250 linii, pierwsza linia `// @summary: …`.
8. Determinizm: seed z `factory.json`; te same wejścia = ten sam przebieg (test `SameSeedSameRun`).

## Model domeny (src/Factory.Core)
- `Equipment` (abstr.): Id, Name, State, FaultModel (MTBF/MTTR), komendy Start/Stop/Reset, czas w stanach.
- `Station : Equipment`: cykl acquire → `Process(item)` → emit; Starved/Blocked; liczniki Processed/Good/Reject.
- Stacje (`Stations/`, klucz `[StationType]`): feeder, filler, capper, labeler, vision, casepacker, palletizer.
- `Conveyor : Equipment`: akumulująca taśma (pitch, length, speed), blokuje stację przed sobą.
- `Line`: stacje w łańcuchu + taśmy; KPI: OEE = A×P×Q (na `oeeStation`), ThroughputPerMin (60 s), Good/Reject.
- `Hall`: strefy (tylko layout) + linie. `SimEngine`: Tick, Command("L1" | "L1/FILL" | "Hall"), Events, Nodes().
- `Layout`: geometria ekranów z manifestu (canvas 1600×900; Optix ×1.2) + lint.
- Stany: Stopped 0, Running 1, Starved 2, Blocked 3, Faulted 4, Maintenance 5; kolory `StatePalette` = `design/theme.json`.

## Jak dodać stację (3 kroki)
1. `src/Factory.Core/Stations/Xyz.cs`: `[StationType("xyz")] class Xyz : Station`, nadpisz `Process`, dodaj `[Signal]`, `FaultCatalog`.
2. `factory.json`: wpis w `stations` (id, type, name, pos [x,y], cycleS, fault, params) + taśmy `from`/`to`.
3. `make check`. FactoryBuilder i podgląd podchwycą stację automatycznie.

## Optix (ścieżki w projekcie)
- `Model/Factory/{Hall|L1|L1/FILL}/<signal>` + `stateColor` (UInt32 ARGB).
- NetLogic runtime `SimulationLogic` w `Model` (poza folderem generowanym): Start/StopLine, ResetFault, SetTimeScale.
- NetLogic design-time `FactoryBuilder`: `Build()` (kopiuje repo `factory.json` do ProjectFiles), `Clean()`.
- Ekrany ręczne raz: `UI/Screens/HallScreen` i `LineScreen`, każdy z Panelem `FactoryContent`.

## Styl HMI (skrót; pełne zasady: docs/hmi-style.md)
Rockwell Process HMI Style Guide / ISA-101: tło `#E0E0E0`, obrys urządzeń `#A0A0A4`, wnętrze = tło, dane `#475CA7`,
Arial. Stany: Stop `#808080`, Praca `#F0F0F0`, oczekiwanie/obsługa `#93C2E4` + tekst. Kolory alarmowe
(`#E22028`, `#EC8629`, `#F5E11B`, `#916AAD`) WYŁĄCZNIE dla alarmów. Ekrany: poziom 1 hala, 2 linia, 3 stacja, 4 diagnostyka.
Przyciski ≥ 40 px, odstęp 10 px. Stan nigdy tylko kolorem.

## Podgląd (Claude Design)
Canvas: https://claude.ai/artifact/5UbGvx3SZaw2ncuJ9pPdrE — artboardy `Main` (hala), `Line1`, `Status`.
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
- `design/canvas/project/Line1.dc.html` — artboard: Linia pakująca 1 — poziom 2
- `design/canvas/project/Main.dc.html` — artboard: Hala — przegląd, poziom 1
- `design/canvas/project/Status.dc.html` — artboard: Stan projektu — poziom 4
- `src/Factory.Core/Factory.Core.csproj` — Pure C# domain + simulation. No FTOptix references allowed.
- `src/Factory.Core/Manifest/FactoryLoader.cs` — Parses + validates factory.json and builds the Hall object graph (stations, conveyors, links).
- `src/Factory.Core/Manifest/FactoryManifest.cs` — DTOs mirroring factory.json (camelCase JSON). Pure data, no behavior.
- `src/Factory.Core/Manifest/Layout.cs` — Screen geometry from factory.json (1600x900 logical canvas): station/conveyor rects + layout lint. Used by Optix builder and preview.
- `src/Factory.Core/Manifest/StationRegistry.cs` — Maps manifest "type" keys to Station classes via [StationType]; the single extension point for new stations.
- `src/Factory.Core/Model/Conveyor.cs` — Accumulating belt: items keep pitch spacing, travel length/speed, block the upstream station when full.
- `src/Factory.Core/Model/Equipment.cs` — Base of every simulated device: identity, commands, fault model, state machine hook, time-in-state.
- `src/Factory.Core/Model/Hall.cs` — Root of the factory: hall metadata, zones (layout only) and production lines.
- `src/Factory.Core/Model/Item.cs` — A product unit flowing through the line (bottle, case or pallet) with quality flags.
- `src/Factory.Core/Model/Line.cs` — Production line: ordered stations + conveyors, line commands, aggregate state and OEE/throughput KPIs.
- `src/Factory.Core/Model/MachineState.cs` — Machine states + shared color palette (used by Optix binder and Design preview).
- `src/Factory.Core/Model/SignalAttribute.cs` — [Signal] marks a property exported to Optix variables, trace.json and the preview.
- `src/Factory.Core/Model/Station.cs` — Template-method work cycle for stations: acquire -> process (subclass) -> emit; tracks starved/blocked.
- `src/Factory.Core/Sim/FaultModel.cs` — Random failures: exponential MTBF on running time, MTTR repair, optional auto-recover.
- `src/Factory.Core/Sim/SimContext.cs` — Per-tick context passed to every node: dt, clock, RNG, event sink, id generator.
- `src/Factory.Core/Sim/SimEngine.cs` — Runs the hall: fixed-step Tick(dt), commands by path ("L1", "L1/FILL"), event log, node enumeration.
- `src/Factory.Core/Sim/SimEvent.cs` — Alarm/event record (fault raised/cleared, maintenance) emitted by the engine.
- `src/Factory.Core/Sim/SimRandom.cs` — Seeded RNG (deterministic runs) with normal, exponential and chance helpers.
- `src/Factory.Core/Stations/Capper.cs` — Screws caps; torque outside tolerance marks the bottle defective.
- `src/Factory.Core/Stations/CasePacker.cs` — Collects bottles into cases (unitsPerCase) and emits one case item per full case.
- `src/Factory.Core/Stations/Feeder.cs` — Source station: creates empty bottles at its cycle rate (infinite supply).
- `src/Factory.Core/Stations/Filler.cs` — Fills bottles from a buffer tank; fill volume is noisy, out-of-tolerance bottles are marked defective.
- `src/Factory.Core/Stations/Labeler.cs` — Applies labels from a roll; random misses mark the bottle defective; roll stock is consumed.
- `src/Factory.Core/Stations/Palletizer.cs` — Line sink: stacks cases on pallets; a full pallet triggers a timed pallet change (Maintenance).
- `src/Factory.Core/Stations/VisionInspector.cs` — Inspects every bottle; anything not filled, capped, labeled and defect-free is rejected.
- `src/Factory.Optix/AlarmGenerator.cs` — One DigitalAlarm per equipment in Alarms/Factory, linked to Model/Factory/<path>/faultActive.
- `src/Factory.Optix/Factory.Optix.csproj` — Cloud compile check of the NetLogic layer against stubs. In Studio these .cs files are linked into NetSolution instead.
- `src/Factory.Optix/FactoryBuilder.cs` — Design-time NetLogic: Build() regenerates types, Model/Factory instances, alarms and screen content from factory.json.
- `src/Factory.Optix/ManifestSource.cs` — Loads ProjectFiles/factory.json inside Optix (builds the Core Hall); design-time sync copies the repo-root manifest in.
- `src/Factory.Optix/ModelGenerator.cs` — Builds Model/Factory: Hall object + one object per line with its stations and conveyors (paths match SimEngine.Nodes()).
- `src/Factory.Optix/NodeUtil.cs` — Small node helpers for generators: ensure/reset folders, clear children, typed UI element factories.
- `src/Factory.Optix/OptixBinder.cs` — Runtime bridge: writes changed [Signal] values (+ stateColor) from SimEngine into Model/Factory variables.
- `src/Factory.Optix/OptixNames.cs` — Optix-side naming: project paths, camelCase variable names, .NET -> OPC UA data type mapping, UI colors.
- `src/Factory.Optix/ScreenGenerator.cs` — Fills HallScreen/LineScreen FactoryContent panels: zones, line tiles, stations and belts at manifest positions (x1.2).
- `src/Factory.Optix/SimulationLogic.cs` — Runtime NetLogic (Model/Factory/SimulationLogic): ticks SimEngine every 100 ms, publishes signals, exposes commands.
- `src/Factory.Optix/TypeGenerator.cs` — Creates one Optix ObjectType per Core class (stations, Conveyor, Line, Hall) with a variable per [Signal].
- `src/Sim.Cli/Checks.cs` — Behavior checks on a finished run: KPI targets, faults present, no dead station, bottle conservation.
- `src/Sim.Cli/LayoutExport.cs` — Writes design/data/layout.json (zones, line rects, station + belt rects) so the preview never re-implements geometry.
- `src/Sim.Cli/Program.cs` — CLI entry: `Sim.Cli [--manifest factory.json] [--minutes 30] [--frame 2] [--out design/data]`; exit 1 if a check fails.
- `src/Sim.Cli/Sim.Cli.csproj` — Console runner: simulates the hall, writes design/data/trace.json + trace-summary.json, checks KPI targets.
- `src/Sim.Cli/TraceRecorder.cs` — Samples all [Signal] values + belt positions every frame into a compact columnar trace.
- `stubs/Optix.Stubs/FTOptix.cs` — Stubs for FTOptix.* namespaces (NetLogic base + tasks, Project, InformationModel, UI items, alarms, ResourceUri).
- `stubs/Optix.Stubs/UAManagedCore.cs` — Stubs for UAManagedCore (nodes, variables, values, NodeId, Log, Color) - only members our code uses.
- `tests/Factory.Tests/CoreTests.cs` — Tests for manifest validation, conveyor/flow behavior, determinism, KPIs, signals and palette.
- `tests/Factory.Tests/Fixtures.cs` — Test helpers: repo paths, loading the real factory.json, building small ad-hoc lines.
- `tests/Factory.Tests/TestRunner.cs` — Minimal test harness: discovers static methods marked [Test], runs them, prints PASS/FAIL, exit code.
- `tools/check_design.py` — Static lint of the Design canvas: canvas.json <-> artboards, required head line, hole syntax, sizes, data files.
- `tools/ctx.py` — Context budget guard + file index: `--check` enforces token/line limits, `--write` refreshes the index in CONTEXT.md.
- `tools/gen_preview.py` — Packs design/data/{layout,trace}.json into design/canvas/project/data/preview.json (columnar, compact) for the Design canvas.
- `tools/gen_status.py` — Builds design/canvas/project/data/status.json (checks, tests, KPIs, backlog, journal, context) for the Status artboard.
- `tools/snapshot.py` — Renders PNG snapshots (hall + line) from preview.json via headless Chromium so the agent can look at its own UI.
- `tools/sync_remote.sh` — Connects the local repo to GitHub when this session can reach it; merges remote state into claude/dev. Prints REMOTE=github|none.
<!-- index:end -->
