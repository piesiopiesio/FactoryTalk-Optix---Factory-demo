# Styl HMI — skrót zasad dla projektu

Źródło: Rockwell Automation, *Process HMI Style Guide* (PROCES-WP023, zgodny z ANSI/ISA-101.01-2015):
https://literature.rockwellautomation.com/idc/groups/literature/documents/wp/proces-wp023_-en-p.pdf
Tokeny w `design/theme.json` (kolory stanów = `StatePalette.cs`, pilnuje test). Ten plik wystarcza — nie czytaj PDF-a.

## Hierarchia ekranów
| Poziom | Cel | U nas |
| --- | --- | --- |
| 1 Przegląd | KPI, 2–3 najważniejsze alarmy, stan głównych urządzeń, bez sterowania | `HallScreen` / artboard `Main` |
| 2 Praca operatora | Zadanie operatora, wszystkie alarmy obszaru, sterowanie | `LineScreen` / artboard `Line1` |
| 3 Szczegóły | Pojedyncza stacja, diagnostyka, sterowanie szczegółowe | faceplate stacji (panel w `Line1`) |
| 4 Wsparcie | Diagnostyka systemu, dokumentacja alarmów, pomoc | `SimSettings`, artboard `Status` |

## Kolory (hex)
- Tło ekranu `#E0E0E0`; panele zakładek `#C0C0C0`; ramki grupujące `#E8E8E8`; separatory `#D8D8D8`.
- Tytuły i etykiety `#3F3F3F`. Linie procesowe i obrysy urządzeń Gray 160 `#A0A0A4` (główne 3 px, pomocnicze 1 px).
- Wnętrze urządzenia = kolor tła (bez gradientów, 3D, zdjęć, animacji dekoracyjnych).
- Stany urządzeń: Stop `#808080`, Praca `#F0F0F0`, Przejściowy/ręczny `#93C2E4`
  (u nas: Brak podaży, Zablokowana, Obsługa — rozróżnione tekstem).
- Alarmy (tylko dla alarmów!): Pilny `#E22028`, Wysoki `#EC8629`, Średni `#F5E11B`, Niski `#916AAD`,
  błąd programu `#000000`, ostrzeżenie `#3F3F3F`. Awaria maszyny = Wysoki; bezpieczeństwo (np. kurtyna) = Pilny.
- Dane na żywo: niebieski `#475CA7` (tło danych `#D4D4D4`); jednostki szare
  (`#919191` w przewodniku; u nas `#767676` dla kontrastu 4.5:1).
- Stan nigdy tylko kolorem: zawsze tekst lub kształt obok.

## Tekst
Bezszeryfowy (Arial). Tytuł ekranu 24 pt bold; tytuł obszaru 12–16 pt bold; dane główne 11 pt bold;
dane drugorzędne 10 pt; etykiety, jednostki, nagłówki 8–10 pt. Bez skrótów (łatwiejsze tłumaczenie).
Liczby wyrównane do prawej, etykiety do lewej, jednostka po prawej stronie wartości.

## Rozmiary i układ
- Przycisk ≥ 40×40 px, nawigacja ≥ 35×35 px, strefa dotyku obiektu ≥ 40×40 px; odstęp 10 px między przyciskami.
- Faceplate: zakładki Home / Diagnostyka / Ustawienia / Alarmy; rozmiar = (liczba kontrolek × 50) + 10 px.
- Nagłówek: mapa nawigacji, status systemu, baner alarmów, powrót do Home.
- Przepływ od lewej do prawej. Grupuj bliskie rzeczy (≥ 4 px, dotyk 10 px).

## Alarmy
- Ikona alarmu przy urządzeniu (kolor priorytetu + numer priorytetu 1–4).
- Niepotwierdzony: miga między kolorem alarmu a szarą ramką; potwierdzony: stała ramka w kolorze alarmu.
- Poziom 1: 2–3 najważniejsze alarmy + informacja, że jest więcej.

## Trendy i KPI
- KPI z kontekstem (zakres docelowy, poprzednia wartość). Sparkline dla trendu bez przybliżania.
- Bar graph: pojemnik + wskaźnik wartości, liczba obok, strefy alarmowe opcjonalnie.
- Odświeżanie 1–2 s.
