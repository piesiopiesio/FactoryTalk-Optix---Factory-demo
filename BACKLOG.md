# BACKLOG

Format: `- [ ] [S|M] Tytuł — kryterium akceptacji`. Kolejność w `## Next` = priorytet.
`[user]` = od użytkownika (agent nie usuwa), `[agent]` = propozycja agenta, `(blocked)` = wstrzymane.

## Next
- [ ] [M] Cykl uzupełniania zbiornika napełniarki — stan `Maintenance` + zdarzenie `LowTank` w przebiegu, test
- [ ] [S] Wymiana rolki etykiet — przestój `Maintenance` etykieciarki, widoczny w OEE i zdarzeniach
- [ ] [S] Artboard `Alarms` — lista zdarzeń z czasem, stacją, czasem trwania; link z `Line1`
- [ ] [M] Wstrzykiwanie awarii — `Cmd.InjectFault` w Core + `SimulationLogic.InjectFault` + przycisk w faceplate podglądu
- [ ] [S] Mikroprzestoje < 30 s — straty wydajności widoczne w OEE (Performance spada o 2–5 pp)
- [ ] [M] Straty OEE per stacja (Pareto) — dane w trace-summary + wykres w `Line1`
- [ ] [M] Model zmian (3 zmiany, planowane przestoje) — czas planowany w OEE
- [ ] [M] SKU 0,5 L / 1,5 L z przezbrojeniem — czasy cyklu z receptury w factory.json
- [ ] [M] DataLogger + `TrendsScreen` w FactoryBuilder — logger OEE/throughput/tankLevel i trend
- [ ] [M] Priorytety alarmów wg przewodnika HMI — priorytet 1–4 w `FaultCatalog` (np. kurtyna świetlna = Pilny), kolor i numer w ikonie
- [ ] [S] Potwierdzanie alarmów — stan niepotwierdzony (miganie ramki) i potwierdzony (stała ramka) w podglądzie i Optix

## Later
- [ ] [M] LineScreen oparty o alias (jeden ekran dla wielu linii; Optix 1.8 `SetDynamicLinkToAlias`)
- [ ] [M] Faceplate w Optix (okno dialogowe z sygnałami stacji) generowany przez FactoryBuilder
- [ ] [M] Strefa Magazyn: palety z paletyzatora trafiają do regału (AGV jako taśma logiczna)
- [ ] [M] Strefa Media: sprężone powietrze i energia per linia (kWh/1000 butelek)
- [ ] [M] Druga linia (procesowa: mieszalnik + CIP) w hali
- [ ] [M] Rozgałęzienia taśm (bufor boczny, odrzut z wizyjnej na osobną taśmę)
- [ ] [S] Formatowanie liczb w Optix (StringFormatter) dla OEE i liczników
- [ ] [M] Serwer OPC UA + MQTT publikujący KPI hali
- [ ] [M] Faceplate z zakładkami Home / Diagnostyka / Ustawienia / Alarmy (przewodnik HMI), rozmiar = kontrolki × 50 + 10
- [ ] [S] Sparkline OEE i wydajności na poziomie 1 (hala), bar graph poziomu zbiornika z limitami
- [ ] [S] Pasek przycisków poziomu 2/3 w nagłówku Optix (nawigacja wg przewodnika HMI)
- [ ] [S] [agent] `snapshot.py`: mini-mapa stacji w kafelku linii na obrazie hali (jak w artboardzie `Main`)
- [ ] [S] [agent] Wejście/wyjście butelek między klatkami w `Line1` (pojawianie się na 0 i znikanie na 1 w trakcie interpolacji, bez skoku co 2 s)
- [ ] [S] [agent] `snapshot.py --sub 0.5`: klatka pośrednia taśm (ta sama reguła co `beltAt` w artboardzie), żeby agent widział interpolację

## Blocked

## Done
- [x] 2026-09-25 Płynna animacja taśm w `Line1` — odtwarzanie 10 fps, interpolacja kinematyczna (prędkość, podziałka), test kolejności pozycji
- [x] 2026-09-25 Szkielet: Core + 7 stacji + taśmy, Sim.Cli, testy, warstwa Optix na stubach, podgląd Design (Hala, Linia 1, Status)
