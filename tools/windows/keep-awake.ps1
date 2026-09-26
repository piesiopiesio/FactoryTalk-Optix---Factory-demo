<#
.SYNOPSIS
  Czuwanie po pobudce: trzyma komputer i ekran włączone przez -Minutes minut, potem pozwala mu zasnąć.
  Uruchamiane przez zadanie „Optix Demo - pobudka” (setup-sleep-wake.ps1).
  Przedłużanie: każde dotknięcie pliku keep-awake.flag obok skryptu = czuwanie do (czas zmiany pliku + 15 min),
  łącznie maks. -MaxHours. Dzięki temu Claude może przedłużyć sesję bez uprawnień administratora.
  Wpisy trafiają do keep-awake.log obok skryptu.
#>
param([int]$Minutes = 60, [int]$MaxHours = 4)

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$log  = Join-Path $here "keep-awake.log"
$flag = Join-Path $here "keep-awake.flag"
function Write-Log([string]$text) {
    Add-Content -Path $log -Encoding UTF8 -Value ("{0:yyyy-MM-dd HH:mm:ss}  {1}" -f (Get-Date), $text)
}

Add-Type -Namespace OptixDemo -Name Power -MemberDefinition @'
[DllImport("kernel32.dll")] public static extern uint SetThreadExecutionState(uint esFlags);
'@
$ES_CONTINUOUS = [uint32]2147483648   # 0x80000000
$ES_SYSTEM     = [uint32]1            # system stays awake (also after an unattended wake, which would sleep again in ~2 min)
$ES_DISPLAY    = [uint32]2            # screen on: Claude sees and clicks Studio

$start    = Get-Date
$hardStop = $start.AddHours($MaxHours)
$until    = $start.AddMinutes($Minutes)
[void][OptixDemo.Power]::SetThreadExecutionState([uint32]($ES_CONTINUOUS -bor $ES_SYSTEM -bor $ES_DISPLAY))
Write-Log ("pobudka: czuwanie do {0:HH:mm}" -f $until)

try {
    while ((Get-Date) -lt $until -and (Get-Date) -lt $hardStop) {
        Start-Sleep -Seconds 30
        if (Test-Path $flag) {
            $extended = (Get-Item $flag).LastWriteTime.AddMinutes(15)
            if ($extended -gt $hardStop) { $extended = $hardStop }
            if ($extended -gt $until) { $until = $extended; Write-Log ("przedłużone do {0:HH:mm}" -f $until) }
        }
    }
}
finally {
    [void][OptixDemo.Power]::SetThreadExecutionState($ES_CONTINUOUS)
    Write-Log "koniec czuwania - komputer może zasnąć"
}
