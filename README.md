# BokningsApi

![Tests](https://github.com/AryanUTV/BokningsAPI/actions/workflows/tests.yml/badge.svg)

Ett REST-API för att boka lokaler, byggt med C# och ASP.NET Core. Systemet hindrar dubbelbokning: en lokal kan inte bokas om den redan är upptagen under den tiden.

## Funktioner

- Skapa och lista lokaler
- Skapa och lista bokningar
- Validering: sluttid måste vara efter starttid, lokalen måste finnas, och bokningar får inte överlappa
- Automatiska tester som körs via GitHub Actions vid varje push

## Teknik

- C# och ASP.NET Core Web API
- Entity Framework Core med SQLite
- xUnit för tester
- GitHub Actions (CI)

## Endpoints

| Metod | Adress | Beskrivning |
|-------|--------|-------------|
| GET | /api/rooms | Hämta alla lokaler |
| POST | /api/rooms | Skapa en lokal |
| GET | /api/bookings | Hämta alla bokningar |
| POST | /api/bookings | Skapa en bokning |

Svarskoder vid skapande av bokning: `201` (skapad), `400` (felaktiga tider), `404` (lokalen finns inte), `409` (lokalen är redan bokad).

## Köra projektet

1. Öppna `BokningsAPI.slnx` i Visual Studio.
2. Öppna Package Manager Console och kör `Update-Database` för att skapa databasen.
3. Starta projektet. Exempel på anrop finns i filen `BokningsApi.http`.

## Köra testerna

Öppna Test Explorer i Visual Studio och välj Run All Tests, eller kör `dotnet test` i terminalen.

## Planerade förbättringar

- Inloggning med JWT
- Ta bort och ändra bokningar
- Docker och deploy till molnet
