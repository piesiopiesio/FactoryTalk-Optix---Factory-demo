# AGENT_DAILY — procedura codziennego przebiegu

Jeden przebieg = jedno zadanie z backlogu, przetestowane i zapisane. Zmieniasz tę procedurę commitem.

## 0. Środowisko
- `dotnet --version` (wymagany .NET 8 SDK). Brak: `sudo apt-get update; sudo apt-get install -y dotnet-sdk-8.0`
  (błąd repozytorium docker w `apt-get update` ignoruj). NuGet jest zablokowany i niepotrzebny (`nuget.config`).
- Python 3 + Playwright (Chromium w `/opt/pw-browsers`) do `make snapshot`.
- Gałąź robocza: `claude/dev` (sesje Claude zawsze mogą wypychać gałęzie `claude/*`). Nigdy nie commituj na `main` —
  Maciek scala `claude/dev` → `main` przez pull request po sprawdzeniu w Studio.
- `bash tools/sync_remote.sh` — łączy z GitHubem, jeśli sesja ma dostęp (scala `github/claude/dev` lub przy pierwszym
  kontakcie `github/main`), i wypisuje `REMOTE=github` albo `REMOTE=none`. Zapamiętaj wynik na krok 7.

## 1. Wczytaj stan (tylko te pliki)
`CONTEXT.md`, `BACKLOG.md`, 3 najnowsze wpisy `JOURNAL.md`, `design/data/trace-summary.json`,
`docs/studio/feedback.md` (jeśli zmieniony od ostatniego wpisu w dzienniku — ma pierwszeństwo przed backlogiem).
Nie czytaj: `optix/**/*.yaml`, `design/data/trace.json`, `design/canvas/project/data/*.json`.

## 2. Zdrowie
`make check`. Czerwono → zadaniem dnia jest naprawa (zapisz przyczynę w dzienniku). Zielono → krok 3.

## 3. Wybór zadania
Pierwsza pozycja `- [ ]` w sekcji `## Next`, która nie ma `(blocked)`.
Szacunek > ~400 zmienionych linii → podziel na 2–3 pozycje w backlogu i zrób pierwszą.
Nie usuwaj i nie przepisuj pozycji oznaczonych `[user]`.

## 4. Realizacja
- Zasady z `CONTEXT.md` (niezmienniki 1–8). C# 10, bez nowych zależności.
- API Optix: sprawdź w NetLogic CheatSheet zanim użyjesz nowego członka
  (`git clone --depth 1 https://github.com/FactoryTalk-Optix/NetLogic_CheatSheet /tmp/cs` i `grep -r` w `/tmp/cs/pages`,
  albo konektor gitmcp, jeśli jest podłączony). Nowy członek API = dopisz minimalny stub w `stubs/Optix.Stubs`.
- Zmiana wyglądu = edytuj artboard w `design/canvas/project/*.dc.html` (holes tylko `{{a.b}}`, logika w `renderVals`)
  i trzymaj się `docs/hmi-style.md` (kolory alarmowe tylko dla alarmów, poziomy 1–4, przyciski ≥ 40 px).
- Nowe zachowanie = test w `tests/Factory.Tests` albo kontrola w `src/Sim.Cli/Checks.cs`.

## 5. Test
`make check`, potem `make snapshot` i obejrzyj `design/snapshots/hall.png` oraz `line1.png` (narzędzie Read).
Sprawdź: nic nie nachodzi na siebie, podpisy mieszczą się, stany i KPI wyglądają wiarygodnie.

## 6. Pamięć projektu
- `BACKLOG.md`: zadanie → `## Done` z datą; 1–2 nowe propozycje na koniec `## Later` z tagiem `[agent]`.
- `JOURNAL.md`: nowy wpis NA GÓRZE `## RRRR-MM-DD` (3–5 punktów: co, wynik KPI, problemy, następne).
  Więcej niż 7 wpisów → najstarsze do `docs/archive/journal-RRRR-MM.md`.
- `python3 tools/ctx.py --write` (indeks w CONTEXT.md). Zmiana architektury → popraw CONTEXT.md.

## 7. Zapis
- `git add -A && git commit -m "daily: <zadanie>"` na `claude/dev`.
- `REMOTE=github` → `git push -u github claude/dev` (zawsze tylko ta gałąź). `REMOTE=none` → pomiń push, zanotuj w raporcie.
- `make bundle` (kopia całej historii do canvasu).

## 8. Publikacja podglądu
Canvas: `https://claude.ai/artifact/5UbGvx3SZaw2ncuJ9pPdrE`, root = `design/canvas` (ścieżki `project/...`).
- Zawsze: `project/data/preview.json`, `project/data/status.json`, `project/data/repo.bundle.b64`
  (`contentType: text/plain` dla `.b64`). Narzędzie odrzuca publikację pliku, którego w tej sesji nie odczytałeś:
  najpierw `read` z `path` dla każdego z nich (duże pliki zapisują się na dysk, nie trafiają do kontekstu).
- Artboard `.dc.html` tylko gdy go zmieniłeś — najpierw odczytaj jego wersję z canvasu (użytkownik mógł edytować) i scal.
- `project/canvas.json` tylko przy dodaniu/przesunięciu artboardu (odczytaj, zmień tylko swoje klucze).

## 9. Raport (ostatnia wiadomość, max 5 linii)
Co zrobione · KPI (OEE, butelki/min, testy) · następne zadanie · blokady / co potrzebne od użytkownika.

## Bariery
Bez force-push, bez zmian na `main`, bez edycji plików Studio, bez sekretów w repo.
To samo zadanie nieudane 2 dni z rzędu → dopisz `(blocked)` + powód i weź następne.
