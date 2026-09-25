# Optix Demo Factory

Demo FactoryTalk Optix: fabryka w jednej hali, dane z modularnej symulacji w C#, rozwijana codziennie przez agenta Claude.

- Plan: dokument „Optix Demo Factory — plan” (Claude Docs)
- Podgląd ekranów: https://claude.ai/artifact/5UbGvx3SZaw2ncuJ9pPdrE (Hala, Linia 1, Stan projektu)
- Mapa kodu dla agenta: `CONTEXT.md` · procedura dnia: `AGENT_DAILY.md` · kolejka: `BACKLOG.md`

## Szybki start
```bash
make check       # build + testy + symulacja 30 min + podgląd + linty
make snapshot    # PNG hali i linii do design/snapshots/
```
Wymaga .NET 8 SDK i Pythona 3 (Playwright tylko do `snapshot`). Bez pakietów NuGet.

Konfiguracja projektu w Studio: `docs/studio-setup.md`.
