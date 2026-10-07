@echo off
rem @summary: Pushes branch claude/dev from repo.bundle (copy from the Claude preview) to GitHub using this PC's git credentials.
rem Usage: put repo.bundle next to this file and double-click. Never touches main, never force-pushes.
setlocal
cd /d "%~dp0"
set REPO=https://github.com/piesiopiesio/FactoryTalk-Optix---Factory-demo.git
where git >nul 2>&1 || (echo Brak gita: zainstaluj Git for Windows ^(git-scm.com^). & pause & exit /b 1)
if not exist repo.bundle (echo Brak pliku repo.bundle obok skryptu. & pause & exit /b 1)
git bundle verify repo.bundle >nul || (echo repo.bundle uszkodzony. & pause & exit /b 1)
if not exist github-push\.git (
  git init -q github-push || (pause & exit /b 1)
  git -C github-push remote add origin %REPO%
)
git -C github-push fetch -q ..\repo.bundle +refs/heads/claude/dev:refs/remotes/bundle/claude-dev || (pause & exit /b 1)
echo Wysylam claude/dev na GitHub (logowanie przez okno Git Credential Manager, jesli zapyta)...
git -C github-push push origin refs/remotes/bundle/claude-dev:refs/heads/claude/dev
if errorlevel 1 (
  echo.
  echo Push odrzucony. Jesli GitHub ma juz inna historie claude/dev, napisz do Claude - scali ja bez nadpisywania.
) else (
  echo.
  echo Gotowe: https://github.com/piesiopiesio/FactoryTalk-Optix---Factory-demo/tree/claude/dev
)
pause
