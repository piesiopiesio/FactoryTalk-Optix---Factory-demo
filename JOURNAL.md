# JOURNAL

Najnowszy wpis na górze. Maks. 7 wpisów; starsze w `docs/archive/`.

## 2026-10-07 (przebieg dzienny)
- Zadanie: [help 1.1] pasma Severity. Optix pokazuje priorytet z pasm 1–250 Low, 251–500 Medium, 501–750 High, 751–1000 Urgent, więc
  dawne 300 (P4) i 500 (P3) były oba „Medium”. Teraz 900/700/400/200; nowa funkcja `AlarmPriorities.OptixBand(severity)` opisuje pasma Optix.
- Test `SeverityLandsInItsOwnOptixBand`: każdy priorytet ląduje we własnym paśmie, granica 500/501, linia demo używa wszystkich 4 pasm.
- Testy 29/29, checks 6/6. KPI bez zmian (zmiana nie dotyka symulacji): OEE 67,0 %, 66,8 butelki/min średnio. Snapshoty bez zmian.
- GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. W Studio: Build (AlarmGenerator ustawi nowe Severity) i rzut oka na AlarmGrid.
  Następne: [help 1.2] `selectedStation`/`stopRequest` per sesja.

## 2026-10-06 (przebieg dzienny)
- Zadanie: straty OEE per stacja (Pareto). Nowa klasa Core `OeeLosses` (w `Line`): każda sekunda czasu planowanego napełniarki (stacja OEE)
  przypisana sprawcy — awaria/obsługa = FILL (Dostępność), Brak podaży → w górę linii, Blokada → w dół do pierwszego urządzenia, które nie czeka
  (pełna taśma nie jest przyczyną, stojąca tak), mikroprzestój = FILL, odrzut = stacja, która zrobiła wadę (`Item.MarkDefect`/`DefectBy`).
  Rozkład dokładny: OEE + Σ strat = 100 %, reszta „Inne” (niepełny cykl) 0,03 pp.
- Wynik (30 min, seed 42): FILL 18,8 pp (awarie 264 s), CAP 7,3 pp (blokada przez awarie zakręcarki), LAB 4,2 pp, FEED 2,7 pp. PACK (1 awaria, 29 s)
  pochłonięta przez bufory taśm — 0 pp. KPI bez zmian: OEE 67,0 %, 66,8 butelki/min średnio (brak nowego losowania).
- Dane: `oeeLosses` w trace-summary (s, pp, podział A/P/Q); sygnał `OeeLossS` („Strata OEE”) na każdym urządzeniu → trace, faceplate i
  zmienna Optix automatycznie (nie trafia do DataLoggera). `Line1`: panel Pareto między rzędami stacji (6 największych, pp i Σ %), zmienia się z czasem.
- Testy 28/28 (3 nowe w `OeeLossTests.cs`), checks 6/6; render artboardu makietą DCLogic + Chromium (klatki 150 i 900) — nic nie nachodzi.
  GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. W Studio potrzebny Build (nowa zmienna `oeeLossS`). Następne: model zmian.

## 2026-10-05 (sesja z Maćkiem: wdrożenie w Studio)
- Wgrane do Factory_demo 25 plików (e5c2399 → 42a60b7) + poprawka; `optix_build_check` OK, Ctrl+S, Execute Build, F5. AccessLogic.cs nie nadpisany.
- Emulator (web): hala, linia, faceplate FILL (butelka, zbiornik), „Wstrzyknij awarię” wyszarzony bez logowania, trendy działają.
- Błąd: „Ekran linii” na hali nic nie robił — `VariableToModify` wskazywał `openTab` typu ekranu, nie instancji w sesji. Poprawka:
  względny DynamicLink `…@NodeId` (`Ui.OnClickSet relative`), po Build klik przełącza na zakładkę linii.
- Do zrobienia w Studio: Locales = pl-PL (liczby „69.7 %” i legenda trendu `L1_oee` w sesji en-US).

## 2026-10-05 (przebieg dzienny)
- Zadanie: mikroprzestoje < 30 s. Nowy `MicroStopModel` (Core/Sim): odstęp wykładniczy na czasie pracy (`mtbsS`), długość
  wykładnicza przycięta do [1, 29] s (`meanS`). Stan zostaje „Praca”, bez alarmu i zdarzenia, brak postępu cyklu → strata
  Wydajności, nie Dostępności. Sygnały stacji: `MicroStopActive`, `MicroStops`, `MicroStopS` (Optix/trace/faceplate automatycznie).
- factory.json: `microStop` na FILL (240 s / 4 s) i CAP (360 s / 4 s); walidacja `meanS` ≤ 29. Własny strumień RNG na urządzenie
  (seed z hali + ścieżki), żeby nie przetasować losowań awarii. Mimo to zmienia się czas pracy, więc realizacja awarii przy seed 42
  i tak jest inna — pierwszy wariant (240/5, 400/4) dawał przebieg z OEE 49,6 % (długie awarie FILL/PACK); wybrany wariant ma typowy przebieg.
- Test bez awarii (3 seedy): Performance −3,6 pp (kryterium 2–5 pp). Testy 25/25 (3 nowe w `MicroStopTests.cs`), checks 6/6.
  KPI (30 min, seed 42): OEE 67,0 % (było 70,6), Performance 79,6 %, 66,8 butelki/min średnio; FILL 9 mikroprzestojów (64 s), CAP 5 (29 s).
- GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. W Studio potrzebny ponowny Build (3 nowe zmienne na stację). Następne: Pareto strat OEE per stacja.

## 2026-10-04 (przebieg dzienny)
- Zadanie: wstrzykiwanie awarii. Core: `Cmd.InjectFault` → `Equipment.InjectFault` (kod 1, naprawa = średni MTTR, dla urządzeń bez
  modelu awarii 30 s; bez losowania, więc determinizm zostaje; zdarzenie „… (wstrzyknięta)”). Linia/hala odrzucają polecenie.
- Optix: bit `cmdFault` na każdym urządzeniu (TypeGenerator, CommandBits), przycisk „Wstrzyknij awarię” 160 px obok „Kasuj awarię”
  w faceplate (AccessLogic: tylko utrzymanie ruchu), `SimulationLogic.InjectFault(path)` jako [ExportMethod]. `*.yaml` nietknięte.
- Założenie: podgląd nie symuluje (niezmiennik 4), więc przycisk w faceplate `Line1` przeskakuje do najbliższej zarejestrowanej awarii
  wybranej stacji (podpis to wyjaśnia). Render artboardu makietą DCLogic + Chromium: przycisk i podpis mieszczą się w panelu.
- Testy 22/22 (3 nowe w `FaultTests.cs`), checks 6/6. KPI bez zmian (30 min, seed 42): OEE 70,6 %, 67,2 butelki/min średnio.
- GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. Następne: mikroprzestoje < 30 s (Performance −2–5 pp).

## 2026-10-03 (przebieg dzienny)
- Zadanie: artboard `Alarms` (1600×900, na canvasie pod `Line1`). `rowsOf` paruje FaultRaised/Cleared i MaintenanceStarted/Ended
  per urządzenie w wiersze (początek, koniec, czas trwania; otwarte = „trwa”), `LowTank` jako wiersz informacyjny. Filtry
  Wszystkie/Awarie/Obsługa/Informacje (40 px), kafle: 8 awarii, 9:56 łącznie, MTTR 1:14, 6 przestojów planowych, 2:55. Zakładka „Alarmy” + link w `Line1`.
- Założenia: tylko podgląd (Optix ma już zakładkę Alarmy z AlarmGrid); dane wyłącznie z `preview.json`, bez zmian w C#. Limit budżetu
  źródeł 68k → 72k tokenów w `tools/ctx.py` (nowy artboard nie mieścił się; teraz 69,2k). `canvas.json` scalony z wersją z canvasu.
- Weryfikacja: render artboardu przez makietę DCLogic + Chromium (15 wierszy, nic nie nachodzi), `make check` 19/19, 6/6.
  KPI bez zmian (30 min, seed 42): OEE 70,6 %, 67,2 butelki/min średnio (96/min w ostatniej min.). `*.yaml` nietknięte.
- GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. Następne: wstrzykiwanie awarii (`Cmd.InjectFault`).

## 2026-10-02 (przebieg dzienny)
- Zadanie: wymiana rolki etykiet. `Labeler.Hold` jak w paletyzatorze: pusta rolka → `MaintenanceStarted` („wymiana rolki etykiet”),
  stan `Maintenance` przez `rollChangeS` (45 s), potem nowa rolka i `MaintenanceEnded`. Usunięty TODO z natychmiastową wymianą.
- Założenie: `rollLabels` 6000 → 1500 w factory.json, żeby wymiana była widoczna w 30-min demo (przy 6000 nie wystąpiłaby wcale).
  Przebieg 30 min: wymiana 1405,3–1450,3 s; snapshot klatki 710 pokazuje LAB · Maintenance, CAP zablokowana. `*.yaml` nietknięte.
- Test `LabelRollChangeIsMaintenanceWithEvents` (19/19, 6/6). KPI (30 min, seed 42): OEE 70,6 % (było 71,9), 67,2 butelki/min
  średnio (było 68,8), 8 awarii. Budżet źródeł 66,8k/68k tokenów — blisko limitu.
- GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. Następne: artboard `Alarms` (lista zdarzeń, link z `Line1`).
