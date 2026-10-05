# JOURNAL

Najnowszy wpis na górze. Maks. 7 wpisów; starsze w `docs/archive/`.

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

## 2026-09-30 (przebieg dzienny)
- Zadanie: cykl uzupełniania zbiornika napełniarki. `Filler.Hold`: bez stałego dopływu; poziom < `lowPct` (20 %) → nowe zdarzenie
  `SimEventKind.LowTank` + `MaintenanceStarted`, stan `Maintenance` aż do `refillToPct` (95 %) przy `refillLps` 12 L/s → `MaintenanceEnded`.
  Parametry w factory.json. Przebieg 30 min: jedno uzupełnianie 1115,7–1165,8 s (50 s), linia za napełniarką głodna, podajnik zablokowany.
- Założenia: uzupełnianie zatrzymuje nalewanie (przestój w Dostępności), `LowTank` to zdarzenie informacyjne, nie alarm Optix
  (propozycja komunikatu w Later). Snapshot z klatki 570 (t=1140 s) pokazuje FILL · Maintenance. `*.yaml` nietknięte.
- Test `FillerTankRefillsInBatchesWithLowTankEvent` (18/18, 6/6). KPI (30 min, seed 42): OEE 71,9 % (było 74,6), 68,8 butelki/min
  średnio (było 71,6), 8 awarii — spadek zgodny z 50 s przestoju na stacji OEE.
- GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. Następne: wymiana rolki etykiet (Maintenance etykieciarki).

## 2026-09-29 (przebieg dzienny)
- Zadanie: kafel linii na hali → ekran linii. Nowy `NavTabs` w Core (kolejność zakładek: Hala, linie, Alarmy, Trendy, Logowanie;
  `IndexOf(L1) = 1`), używany przez `ScreenGenerator`. Kafel ma przezroczysty cel kliknięcia i przycisk „Ekran linii” (web nie
  dostarcza kliknięć w Rectangle); oba ustawiają `HallScreen/openTab`, a `AccessLogic` (co 0,5 s) ustawia `MainNav.CurrentTabIndex`.
- Założenie (niesprawdzone w Studio): zmienna ekranu jest per sesja (ekran instancjonowany w MainNav sesji), więc klik nie przełącza
  zakładki innym klientom; zmienna w Model przełączałaby wszystkich. Weryfikacja jako pozycja [studio] w Later. `*.yaml` nietknięte.
- Test `HallTileOpensItsLineTab` (17/17, 6/6). KPI bez zmian (30 min, seed 42): OEE 74,6 %, 71,6 butelki/min średnio (96/min w ostatniej min.), 9 awarii.
- GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. Następne: cykl uzupełniania zbiornika napełniarki (Maintenance + LowTank).

## 2026-09-28 (przebieg dzienny)
- Zadanie: separator tysięcy po polsku w Optix. StringFormatter formatuje liczby wg locale sesji (en-US → „1,612”), więc zamiast
  zmieniać formaty `{0}` ustawiam locale: `SecurityGenerator` nadaje kontom demo `User.LocaleId = pl-PL` (API z CheatSheet
  users-groups; dopisane do stubów). Sesja bez logowania bierze locale projektu → jednorazowy krok w Studio (docs/studio-setup.md).
- Założenia (niesprawdzone w Studio): StringFormatter respektuje locale sesji; właściwość projektu „Locales” ustala locale Anonymous.
  Weryfikacja jako pozycja [studio] w Later. Plików `*.yaml` nie ruszam.
- `make check` zielony (16/16, 6/6). KPI bez zmian (30 min, seed 42): OEE 74,6 %, 71,6 butelki/min średnio (96/min w ostatniej min.), 9 awarii.
- GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. Następne: kliknięcie kafla linii na hali → ekran linii.
