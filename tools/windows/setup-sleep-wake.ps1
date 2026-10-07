<#
.SYNOPSIS
  Uśpienie zamiast wyłączania + codzienna pobudka, żeby Claude mógł pracować w Optix Studio bez Ciebie przy biurku.
  Uruchom raz jako administrator (PowerShell -> Uruchom jako administrator):
    powershell -ExecutionPolicy Bypass -File .\setup-sleep-wake.ps1 -TestInMinutes 3
  Potem uśpij komputer (Start -> Zasilanie -> Uśpij); po ~3 min powinien się obudzić, wpis w keep-awake.log.

  Co ustawia (tylko zasilanie sieciowe, bieżący plan):
    - uśpienie po -SleepAfterMinutes bezczynności, bez hibernacji, przycisk zasilania = uśpienie
    - czasomierze wybudzania włączone
    - bez hasła po wybudzeniu (zablokowany ekran blokuje kliknięcia Claude) - każdy przy biurku widzi pulpit
    - karta przewodowa: budzenie tylko pakietem Magic Packet (WoL ze snu), wypisuje MAC
    - zadanie „Optix Demo - pobudka”: codziennie o -WakeAt budzi PC i uruchamia keep-awake.ps1 na -AwakeMinutes
  Cofnięcie zadań: -Remove (ustawienia zasilania zmienia się w Opcjach zasilania).
#>
[CmdletBinding()]
param(
    [string]$WakeAt = "07:00",          # poranny przebieg w chmurze startuje 06:47 i trwa ~5 min
    [int]$AwakeMinutes = 60,
    [int]$SleepAfterMinutes = 30,
    [int]$TestInMinutes = 0,
    [switch]$Remove
)
$ErrorActionPreference = "Stop"
$here      = Split-Path -Parent $MyInvocation.MyCommand.Path
$keepAwake = Join-Path $here "keep-awake.ps1"
$taskName  = "Optix Demo - pobudka"
$testName  = "Optix Demo - pobudka (test)"

function Step([string]$t) { Write-Host "`n== $t" -ForegroundColor Cyan }
function Ok([string]$t)   { Write-Host "  OK  $t" -ForegroundColor Green }
function Warn([string]$t) { Write-Host "  !!  $t" -ForegroundColor Yellow }

$identity = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
if (-not $identity.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Uruchom PowerShell jako administrator."
}
Unregister-ScheduledTask -TaskName $testName -Confirm:$false -ErrorAction SilentlyContinue
if ($Remove) {
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue
    Ok "Zadania pobudki usunięte"
    return
}
if (-not (Test-Path $keepAwake)) { throw "Brak $keepAwake - musi leżeć obok tego skryptu." }

Step "Stany uśpienia dostępne w tym komputerze"
$available = ((powercfg /a | Out-String).Trim() -split "\r?\n\s*\r?\n")[0]   # 1st paragraph = available states
Write-Host $available
if ($available -match "S3") { Ok "Klasyczne uśpienie S3 - najpewniejsze budzenie" }
elseif ($available -match "S0") { Warn "Modern Standby (S0) - budzenie zwykle działa, ale koniecznie zrób test (-TestInMinutes 3)" }
else { Warn "Nie widzę uśpienia S3 ani S0 - sprawdź sterowniki grafiki/chipsetu i ustawienia BIOS" }

Step "Plan zasilania (zasilanie sieciowe)"
$settings = @(
    [pscustomobject]@{ Sub = "SUB_SLEEP";   Key = "STANDBYIDLE";   Value = $SleepAfterMinutes * 60; Info = "uśpienie po $SleepAfterMinutes min bezczynności" }
    [pscustomobject]@{ Sub = "SUB_SLEEP";   Key = "HIBERNATEIDLE"; Value = 0; Info = "bez przechodzenia w hibernację (z niej WoL działa gorzej)" }
    [pscustomobject]@{ Sub = "SUB_SLEEP";   Key = "RTCWAKE";       Value = 1; Info = "czasomierze wybudzania włączone" }
    [pscustomobject]@{ Sub = "SUB_NONE";    Key = "CONSOLELOCK";   Value = 0; Info = "bez hasła po wybudzeniu" }
    [pscustomobject]@{ Sub = "SUB_BUTTONS"; Key = "PBUTTONACTION"; Value = 1; Info = "przycisk zasilania = uśpienie" }
)
foreach ($s in $settings) {
    powercfg /setacvalueindex SCHEME_CURRENT $s.Sub $s.Key $s.Value
    if ($LASTEXITCODE -eq 0) { Ok $s.Info } else { Warn "$($s.Key): powercfg zwrócił $LASTEXITCODE" }
}
powercfg /setactive SCHEME_CURRENT

Step "Karta przewodowa: Wake-on-LAN ze snu (sieć zniknie na kilka sekund)"
$wired = @(Get-NetAdapter -Physical | Where-Object { $_.PhysicalMediaType -eq "802.3" })
if ($wired.Count -eq 0) {
    Warn "Brak karty przewodowej - WoL przez Wi-Fi zwykle nie działa. Pobudka o stałej godzinie działa i tak."
}
foreach ($nic in $wired) {
    try {
        Set-NetAdapterAdvancedProperty -Name $nic.Name -RegistryKeyword "*WakeOnMagicPacket" -RegistryValue 1
        Ok "$($nic.Name): Wake on Magic Packet"
    } catch { Warn "$($nic.Name): sterownik nie ma opcji *WakeOnMagicPacket ($($_.Exception.Message))" }
    try {
        Set-NetAdapterPowerManagement -Name $nic.Name -WakeOnMagicPacket Enabled -WakeOnPattern Disabled
        Ok "$($nic.Name): budzi tylko Magic Packet (nie zwykły ruch w sieci)"
    } catch { Warn "$($nic.Name): zarządzanie energią karty ($($_.Exception.Message))" }
    powercfg /deviceenablewake "$($nic.InterfaceDescription)" | Out-Null   # no 2>: in PS 5.1 + Stop it would throw
    $state = if ($nic.Status -eq "Up") { "podłączona" } else { "bez kabla" }
    Ok "$($nic.Name) ($state) - MAC do aplikacji WoL: $($nic.MacAddress)"
}

function Set-WakeTask([string]$name, $trigger, [int]$minutes) {
    $arguments = "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$keepAwake`" -Minutes $minutes"
    $action    = New-ScheduledTaskAction -Execute "powershell.exe" -Argument $arguments -WorkingDirectory $here
    $options   = New-ScheduledTaskSettingsSet -WakeToRun -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries `
                   -MultipleInstances IgnoreNew -ExecutionTimeLimit (New-TimeSpan -Hours 5)
    $principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" -LogonType Interactive -RunLevel Limited
    Register-ScheduledTask -TaskName $name -Action $action -Trigger $trigger -Settings $options -Principal $principal -Force | Out-Null
}

Step "Zadanie: codzienna pobudka o $WakeAt, czuwanie $AwakeMinutes min"
Set-WakeTask $taskName (New-ScheduledTaskTrigger -Daily -At $WakeAt) $AwakeMinutes
Ok "Zadanie „$taskName” zapisane (Harmonogram zadań)"
if ($TestInMinutes -gt 0) {
    $at = (Get-Date).AddMinutes($TestInMinutes)
    Set-WakeTask $testName (New-ScheduledTaskTrigger -Once -At $at) 10
    Ok ("TEST: pobudka o {0:HH:mm}. Teraz uśpij komputer (Start -> Zasilanie -> Uśpij) i poczekaj." -f $at)
}

Step "Kontrola: zaplanowane wybudzenia"
powercfg /waketimers

Step "Pamiętaj"
Write-Host "  - Wyłączaj przez Uśpij, nie Zamknij: z pełnego wyłączenia zadanie komputera nie obudzi."
Write-Host "  - Zostaw otwarte: Claude Desktop, Optix Studio z Factory_demo (mostek uruchomiony). Po uśpieniu wszystko czeka."
Write-Host "  - Restart po aktualizacji Windows zostawia ekran logowania - wtedy pobudka nic nie da, zaloguj się przy okazji."
Write-Host "  - Log czuwania: $(Join-Path $here 'keep-awake.log')"
