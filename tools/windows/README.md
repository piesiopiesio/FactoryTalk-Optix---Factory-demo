# Komputer Maćka: uśpienie zamiast wyłączania + codzienna pobudka

Cel: Claude może pracować w Optix Studio (build, Execute, emulator, zrzuty), gdy Maćka nie ma przy biurku.
Poranny przebieg w chmurze (06:47) komputera nie potrzebuje; PC jest potrzebny tylko do pracy w Studio.

## Jak to działa
- PC nie jest wyłączany, tylko **usypiany** (pobór ok. 2–5 W). Sesja Windows, Claude Desktop, ftx-mcp, Studio z mostkiem
  i emulator czekają otwarte, więc po pobudce nic nie trzeba uruchamiać.
- Zadanie Harmonogramu „Optix Demo - pobudka” (z opcją *Wybudź komputer*) o 07:00 uruchamia `keep-awake.ps1`:
  komputer i ekran zostają włączone 60 min (bez tego Windows po pobudce bez użytkownika zasypia po ~2 min).
- Claude przedłuża czuwanie, dotykając pliku `keep-awake.flag` obok skryptu (+15 min od dotknięcia, maks. 4 h).
  Przebieg widać w `keep-awake.log`.
- Budzenie na żądanie (Wake-on-LAN) działa ze snu dużo pewniej niż z wyłączenia, ale pakiet musi wysłać urządzenie
  w domowej sieci (aplikacja WoL w telefonie w domowym Wi-Fi, router z funkcją WoL albo Raspberry Pi + Tailscale).

## Instalacja (raz, przy komputerze)
1. Skrypty leżą w `Factory_demo\Narzedzia\` (wgrywa je Claude) albo w repo: `tools/windows/`.
2. PowerShell **jako administrator**:
   `powershell -ExecutionPolicy Bypass -File "<folder>\setup-sleep-wake.ps1" -TestInMinutes 3`
3. Start → Zasilanie → **Uśpij**. Po ~3 min PC powinien się obudzić, w `keep-awake.log` pojawi się „pobudka”.
4. Parametry: `-WakeAt 07:00 -AwakeMinutes 60 -SleepAfterMinutes 30`; cofnięcie zadań: `-Remove`.

## Kompromisy i pułapki
- Bez hasła po wybudzeniu (`CONSOLELOCK=0`): zablokowany ekran blokuje kliknięcia Claude, ale każdy przy biurku widzi pulpit.
- Przycisk zasilania usypia zamiast wyłączać (`PBUTTONACTION=1`).
- Z **pełnego wyłączenia** zadanie nie obudzi PC (do tego potrzebny BIOS: RTC alarm albo gniazdko + „Power On after AC loss”).
- Restart po aktualizacji Windows zostawia ekran logowania → pobudka nic nie da, dopóki Maciek się nie zaloguje
  (ew. automatyczne logowanie, Sysinternals Autologon - świadomie, bo obniża bezpieczeństwo).
- Modern Standby (S0) zamiast S3: budzenie zwykle działa, ale test z kroku 3 jest obowiązkowy.

## Wypchnięcie na GitHub z PC
`wypchnij-na-github.cmd` + `repo.bundle` (kopia z podglądu) w jednym folderze → dwuklik. Wysyła tylko gałąź `claude/dev`
(nigdy `main`, bez force) poświadczeniami gita z tego komputera. Potrzebny Git for Windows.
