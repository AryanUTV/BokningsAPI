# BokningsApi

![Tests](https://github.com/AryanUTV/BokningsAPI/actions/workflows/tests.yml/badge.svg)

Ett REST-API för att boka lokaler, byggt med C# och ASP.NET Core. Man registrerar sig, loggar in och bokar lokaler. Systemet hindrar dubbelbokning: en lokal kan inte bokas om den redan är upptagen under den tiden.

**Live:** https://bokningsapi.onrender.com (prova `/api/rooms`)

> Demon körs på en gratisnivå som stängs av vid inaktivitet, så första anropet kan ta upp till en minut. Databasen nollställs vid omstart.

## Funktioner

- Registrering och inloggning med JWT. Lösenord sparas som hash.
- Skapa och lista lokaler
- Skapa, hämta och ta bort bokningar
- Bokningar kopplas till den inloggade användaren, och man kan bara ta bort sina egna
- Validering: sluttid måste vara efter starttid, lokalen måste finnas, och bokningar får inte överlappa
- Automatiska tester som körs via GitHub Actions vid varje push

## Teknik

- C# och ASP.NET Core Web API
- Entity Framework Core med SQLite
- JWT-autentisering
- xUnit för tester
- GitHub Actions (CI)
- Docker, deploy på Render

## Endpoints

| Metod | Adress | Kräver inloggning | Beskrivning |
|-------|--------|-------------------|-------------|
| POST | /api/auth/register | Nej | Skapa användare |
| POST | /api/auth/login | Nej | Logga in och få en token |
| GET | /api/rooms | Nej | Hämta alla lokaler |
| POST | /api/rooms | Nej | Skapa en lokal |
| GET | /api/bookings | Nej | Hämta alla bokningar |
| GET | /api/bookings/{id} | Nej | Hämta en bokning |
| POST | /api/bookings | Ja | Skapa en bokning |
| DELETE | /api/bookings/{id} | Ja | Ta bort sin egen bokning |

Svarskoder vid skapande av bokning: `201` (skapad), `400` (felaktiga tider), `401` (inte inloggad), `404` (lokalen finns inte), `409` (lokalen är redan bokad).

Skicka token som en header: `Authorization: Bearer <token>`.

## Köra projektet lokalt

1. Öppna `BokningsAPI.slnx` i Visual Studio.
2. Lägg en lång slumpmässig text som `Jwt:Key` i User Secrets (högerklicka på projektet, Manage User Secrets).
3. Starta projektet. Databasen skapas automatiskt. Exempel på anrop finns i filen `BokningsApi.http`.

## Köra testerna

Öppna Test Explorer i Visual Studio och välj Run All Tests, eller kör `dotnet test` i terminalen.

## Möjliga förbättringar

- Byta SQLite mot PostgreSQL så att data överlever omstarter
- Skydda skapande av lokaler med inloggning
- Tester för inloggning och registrering
