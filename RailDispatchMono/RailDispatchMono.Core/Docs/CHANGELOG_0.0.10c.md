# [0.0.10c] — Station Hover UI

**Data:** 2026-08-31

## Station UI / Tooltip

- Dodano tooltip stacji wyświetlany po najechaniu kursorem na komórkę stacji.
- Tooltip jest renderowany w przestrzeni ekranu, analogicznie do istniejącego UI pociągu.
- Tooltip automatycznie przesuwa się na drugą stronę kursora, jeżeli grozi mu wyjście poza ekran.
- Pierwsza linia tooltipa identyfikuje obiekt jako `STACJA`.
- Wyświetlana jest nazwa stacji oraz skrócone ID.

## Passenger Information

Tooltip pokazuje aktualne informacje o pasażerach oczekujących na stacji:

- liczba oczekujących pasażerów,
- liczba różnych stacji docelowych wśród oczekujących pasażerów.

Dodatkowo pokazuje:

- czy stacja obsługuje ruch pasażerski,
- czas postoju pociągu na stacji.

Dane są pobierane bezpośrednio z `StationController.Passengers`, więc tooltip nie utrzymuje własnego, rozjeżdżającego się stanu.

## Integration

- Wykorzystano istniejący `StationController` należący do `TrainManager`.
- Nie utworzono drugiego systemu zarządzania pasażerami.
- Tooltip jest obsługiwany przez `InputManager` po zakończeniu renderowania świata, dzięki czemu pozostaje niezależny od zoomu i przesuwania kamery.
- Istniejące stawianie i usuwanie stacji pozostaje bez zmian.
- Istniejący tooltip pociągu pozostaje niezależnym elementem UI.

## Controls

- Brak nowych klawiszy.
- Tooltip pojawia się automatycznie po najechaniu kursorem na stację.

## Technical Notes

- Czcionka `Arial24` jest pobierana leniwie z `ContentManager`, aby nie próbować ładować zasobów przed `LoadContent()`.
- Tekst tooltipa korzysta z rzeczywistych wymiarów `SpriteFont`, zamiast stałego przybliżenia szerokości znaków.
- Tekstura 1×1 używana jako tło i ramka jest tworzona jednokrotnie i ponownie wykorzystywana.

## Known Limitations

- Tooltip pokazuje agregaty pasażerów, a nie pełną listę indywidualnych pasażerów.
- Szczegółowe rozbicie według stacji docelowej może zostać dodane później.
- Nie dodano jeszcze interaktywnego panelu stacji ani konfiguracji nazwy/czasu postoju z poziomu UI.
