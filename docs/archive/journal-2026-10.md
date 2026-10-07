# JOURNAL — archiwum 2026-10

## 2026-10-02 (przebieg dzienny)
- Zadanie: wymiana rolki etykiet. `Labeler.Hold` jak w paletyzatorze: pusta rolka → `MaintenanceStarted` („wymiana rolki etykiet”),
  stan `Maintenance` przez `rollChangeS` (45 s), potem nowa rolka i `MaintenanceEnded`. Usunięty TODO z natychmiastową wymianą.
- Założenie: `rollLabels` 6000 → 1500 w factory.json, żeby wymiana była widoczna w 30-min demo (przy 6000 nie wystąpiłaby wcale).
  Przebieg 30 min: wymiana 1405,3–1450,3 s; snapshot klatki 710 pokazuje LAB · Maintenance, CAP zablokowana. `*.yaml` nietknięte.
- Test `LabelRollChangeIsMaintenanceWithEvents` (19/19, 6/6). KPI (30 min, seed 42): OEE 70,6 % (było 71,9), 67,2 butelki/min
  średnio (było 68,8), 8 awarii. Budżet źródeł 66,8k/68k tokenów — blisko limitu.
- GitHub niedostępny (REMOTE=none) → tylko kopia w canvasie. Następne: artboard `Alarms` (lista zdarzeń, link z `Line1`).
