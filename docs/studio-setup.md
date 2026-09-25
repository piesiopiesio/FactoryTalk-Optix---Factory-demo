# Konfiguracja w FactoryTalk Optix Studio (jednorazowo)

Wymaga Optix Studio 1.4+ (NetSolution na .NET 6 lub 8). Kod używa C# 10, więc zadziała na obu.

1. Sklonuj repo i przełącz się na gałąź `claude/dev`.
2. W Studio utwórz nowy projekt `OptixDemoFactory` w folderze `optix/` repozytorium
   (wynik: `optix/OptixDemoFactory/OptixDemoFactory.optix`).
3. Zamknij Studio. W pliku `optix/OptixDemoFactory/ProjectFiles/NetSolution/OptixDemoFactory.csproj`
   dodaj przed `</Project>`:

```xml
<ItemGroup>
  <Compile Include="..\..\..\..\src\Factory.Core\**\*.cs"
           Exclude="..\..\..\..\src\Factory.Core\obj\**;..\..\..\..\src\Factory.Core\bin\**"
           LinkBase="FactoryCore" />
  <Compile Include="..\..\..\..\src\Factory.Optix\*.cs" LinkBase="FactoryOptix" />
</ItemGroup>
```

4. Otwórz projekt. W `UI/Screens` utwórz ekrany `HallScreen` i `LineScreen`; w każdym Panel `FactoryContent`
   (wyrównanie Stretch/Stretch). Dodaj je do nawigacji w `MainWindow`.
5. W folderze NetLogic projektu dodaj **design-time** NetLogic o nazwie `FactoryBuilder`,
   a w `Model` **runtime** NetLogic `SimulationLogic`. Studio utworzy puste pliki `.cs` o tych nazwach
   w NetSolution — usuń je (klasy przychodzą z linków `FactoryOptix`).
6. Uruchom metodę `FactoryBuilder.Build` (prawy klik na NetLogic → Execute). Skopiuje `factory.json` z repo do
   `ProjectFiles` i wygeneruje typy, instancje, alarmy i zawartość ekranów.
7. Uruchom emulator. Zapisz zrzuty ekranu w `docs/studio/` i uwagi w `docs/studio/feedback.md`, potem commit.

Plan B, gdy Studio nadpisze `.csproj`: skopiuj pliki z `src/Factory.Core` i `src/Factory.Optix` do NetSolution
(skrypt w backlogu), ale źródłem prawdy zostaje `src/`.
