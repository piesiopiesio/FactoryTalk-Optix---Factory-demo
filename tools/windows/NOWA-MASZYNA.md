# Optix Demo Factory na nowym komputerze

Lista kroków, żeby nowy PC przejął rolę starego: Studio z projektem, ftx-mcp, dostęp Claude z chmury, pobudka 07:00.

## Co NIE wymaga przenoszenia (żyje na koncie / w sieci)
- Kod: GitHub `piesiopiesio/FactoryTalk-Optix---Factory-demo`, gałąź `claude/dev` (+ kopia `repo.bundle.b64` w artefakcie
  https://claude.ai/artifact/5UbGvx3SZaw2ncuJ9pPdrE).
- Plan projektu: dokument „Optix Demo Factory — plan” (Claude Docs).
- Rutyny w claude.ai: „codzienny przebieg” 06:47 (chmura, PC niepotrzebny) i „kontrola w Studio” 07:12 (wymaga PC).
- Konektory konta: Claude Docs, Claude Code Remote.

## Co jest TYLKO na starym PC (trzeba przenieść)
- Folder projektu Studio `Documents\Rockwell Automation\FactoryTalk Optix\Projects\Factory_demo` — w repo go NIE ma
  (pliki `*.yaml`, elementy z Template Library, Locales, lokalny git Studio). W nim też:
  `Narzedzia\deployed-commit.txt` (ostatnio wgrany commit), `Narzedzia\*.ps1`, `demo-users.json` (konta demo).
- Instalacja **ftx-mcp** (usługa lokalna + wpis w Claude Desktop) — na nowym PC instaluje się ją od nowa (krok 3).
- Zadania Harmonogramu „Optix Demo - pobudka”, ustawienia zasilania, poświadczenia gita (Git Credential Manager).

## 1. Instalacja programów
1. **FactoryTalk Optix Studio 1.7.5.13** — ta sama wersja (NetSolution net8.0; inna wersja = inne API i stuby).
2. **Claude Desktop** — zalogowany na to samo konto claude.ai.
3. **Google Chrome** (podgląd emulatora przez CDP), **Git for Windows** (skrypt `wypchnij-na-github.cmd`).
4. Opcjonalnie do pracy z kodem lokalnie: .NET 8 SDK, Python 3, `make` (najprościej WSL Ubuntu: `make check`).

## 2. Projekt Optix
1. Skopiuj CAŁY folder `Factory_demo` ze starego PC (z ukrytym `.git`) do
   `Documents\Rockwell Automation\FactoryTalk Optix\Projects\` na nowym.
2. Otwórz go w Studio → Ctrl+S → w Output „User .NET solution built successfully”.
3. Kopii nie da się zrobić? Projekt od zera wg `docs/studio-setup.md`: nowy projekt `Factory_demo`, NetLogic
   `FactoryBuilder` (design-time) i `StudioMCPBridge`, elementy Template Library z listy, kod przez `make optix`,
   potem Execute **Build** i **CreateDemoUsers**. Uwaga: przepadną ręczne zmiany w Studio.

## 3. MCP
**ftx-mcp** (https://github.com/asqi-carter/ftx-mcp, MIT; narzędzia `optix_build_check`, `optix_emulator`,
`optix_observe`, `optix_interact`, `optix_bridge_*`, `optix_status`). Działa lokalnie: usługa `127.0.0.1:8765` (panel `/ui`),
MCP `127.0.0.1:8766/mcp`, most w Studio `127.0.0.1:8768`, emulator web `localhost:8081`, Chrome z CDP do podglądu.
1. Zwykły PowerShell (NIE terminal wewnątrz Claude Desktop ze Sklepu — setup odmówi):
   ```powershell
   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
   git clone -b v1.0.7 https://github.com/asqi-carter/ftx-mcp.git   # ta sama wersja co most w projekcie
   cd ftx-mcp
   .\bootstrap\setup.ps1          # Python 3.12, venv, Tesseract OCR, zadanie Chrome-CDP (doinstaluje przez winget)
   .\bootstrap\services.ps1 start
   .\bootstrap\services.ps1 status # + http://127.0.0.1:8765/ui
   .\bootstrap\setup-mcp-client.ps1 -WriteConfig   # wpis do Claude Desktop
   ```
   Claude Desktop ze Sklepu Microsoft potrzebuje też Node.js (`winget install OpenJS.NodeJS.LTS`, konfiguracja używa `npx mcp-remote`).
   Domyślnie bez tokena (tylko loopback) — nie włączaj `-EnableAuth`.
2. Zamknij Claude Desktop całkiem (Menedżer zadań) i uruchom ponownie → poproś Claude: „run optix_status(action='doctor')”.
   Uprawnienia narzędzi: Ustawienia → Connectors → ftx-mcp.
3. Most w Studio: NetLogic `StudioMCPBridge` jest już w projekcie (wersja 1.0.7). Po każdym otwarciu projektu
   prawy klik → **Execute StartBridge** (pierwszy raz Studio pyta o zgodę) → w Output `listening on http://127.0.0.1:8768`.
   `SetupProject` (strona web na 8081) projekt ma już zrobione.
4. Nowsza wersja (v1.0.8+): klon bez `-b v1.0.7` i wklej nowy `studio-bridge/StudioMCPBridge.cs` do NetLogic
   `StudioMCPBridge` (Ctrl+S, potem StopBridge/StartBridge) — inaczej część wywołań odmówi (`invoke_unsupported_bridge`).
   Plik mostu w projekcie jest poza repo: nie nadpisuj go z `make optix`.
5. Odinstalowanie na starym PC: `.\bootstrap\uninstall.ps1` (z `-All` usuwa też venv i profil Chrome).

**Dostęp Claude z chmury do PC** (narzędzia `mcp__remote-devices__*`: pliki, sterowanie ekranem, ftx-mcp):
1. W Claude Desktop na nowym PC włącz to samo udostępnianie komputera sesjom w chmurze co na starym.
2. Na starym PC wyłącz je (albo usuń urządzenie), żeby rutyna nie trafiała na stary komputer.
3. Przy pierwszej kontroli Claude poprosi o dostęp do okien „FactoryTalk Optix Studio 1.7.5.13” i „textinputhost.exe” — zatwierdź.

**Opcjonalnie**: konektor gitmcp do NetLogic CheatSheet (agent ma też zapas: `git clone` CheatSheet).

## 4. Rutyna „kontrola w Studio”
Jej prompt ma zaszytą ścieżkę `C:\Users\Maciej Piesio\Documents\...\Factory_demo`. Inna nazwa użytkownika lub folderu →
poproś Claude: „zmień ścieżkę w rutynie kontrola w Studio na …”. Przy nieosiągalnym PC rutyna sama się wstrzymuje
(`device_absent`), a poranny przebieg 06:47 włącza ją ponownie.

## 5. Uśpienie i pobudka 07:00
PowerShell jako administrator: `powershell -ExecutionPolicy Bypass -File "<folder>\setup-sleep-wake.ps1" -TestInMinutes 3`,
uśpij PC, sprawdź wpis „pobudka” w `keep-awake.log` (szczegóły i pułapki: `README.md` obok).
Na starym PC: `setup-sleep-wake.ps1 -Remove`.

## 6. Sprawdzenie końcowe
1. Studio otwarte z `Factory_demo`, Execute StartBridge, Claude Desktop z ftx-mcp i udostępnianiem włączonym.
2. Poproś Claude w sesji z dostępem do PC: „uruchom optix_build_check i emulator, zrób zrzut Hali” — ma być 0 błędów i żywe dane.
3. Poproś o ręczne odpalenie rutyny „Optix Demo Factory — kontrola w Studio” i przeczytaj raport.
4. Czarne zrzuty (tylko pasek zadań) po starcie PC: Win+Ctrl+Shift+B.
