# JOURNAL

Najnowszy wpis na górze. Maks. 14 wpisów; starsze w `docs/archive/`.

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
