# 🎭 More.com Theater Scraper API

A .NET Web API that uses **Microsoft Playwright** to scrape theater event listings from [more.com](https://www.more.com/el/theater/). It supports filtering by **category** and **location**, and returns structured event data including titles, descriptions, dates, images (as Base64), and GPS coordinates.

---

## Features

- Scrapes live theater listings from more.com using a real headless browser
- Filter events by **genre/category** (e.g. Drama, Comedy, Musical)
- Filter events by **location/region** (e.g. Athens, Thessaloniki, Crete)
- Returns rich per-event data: title, description, date, duration, venue, map URL, coordinates, and image (URL + Base64)
- Handles cookie consent popups and dynamic page loads automatically
- Built with ASP.NET Core and Swagger UI

---

## Tech Stack

- [.NET 8+](https://dotnet.microsoft.com/) — ASP.NET Core Web API
- [Microsoft Playwright for .NET](https://playwright.dev/dotnet/) — headless browser automation
- Swagger / OpenAPI — interactive API docs

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Playwright browsers installed

### Installation

```bash
# Clone the repository
git clone https://github.com/your-username/your-repo-name.git
cd your-repo-name

# Restore dependencies
dotnet restore

# Install Playwright browsers
dotnet build
pwsh bin/Debug/net8.0/playwright.ps1 install
```

### Running the API

```bash
dotnet run
```

The API will be available at:
- `http://localhost:5273`
- `https://localhost:7228`

Swagger UI is available at: `https://localhost:7228/swagger`

---

## API Reference

### `POST /api/scraper/scrape`

Scrapes theater events from more.com with optional filters.

**Request Body**

```json
{
  "location": "Αττική",
  "category": "Κωμωδία"
}
```

Both fields are optional. Omitting them scrapes all available events.

**Response**

```json
{
  "url": "https://www.more.com/el/theater/",
  "message": "Successfully scraped 12 events.",
  "totalEventsScraped": 12,
  "events": [
    {
      "title": "Event Title",
      "url": "https://www.more.com/...",
      "imageUrl": "https://...",
      "imageBase64": "...",
      "locationName": "Venue Name, Athens",
      "about": "Event description...",
      "date": "01/06/2025 - 30/06/2025",
      "duration": "1:30",
      "mapUrl": "https://maps.google.com/...",
      "coordinates": "37.9755, 23.7348"
    }
  ]
}
```

---

## Supported Filter Values

### Categories (`category`)

| Greek Name | Description |
|---|---|
| Δράμα | Drama |
| Κωμωδία | Comedy |
| Μουσικό | Musical |
| Μουσικό Θέατρο | Music Theater |
| Παιδικά | For Kids |
| Αρχαίο Δράμα | Ancient Drama |
| Τραγωδία | Tragedy |
| Κλασικό έργο | Classical Work |
| Κοινωνικό Δράμα | Social Drama |
| Μαύρη Κωμωδία | Black Comedy |
| Μονόλογος | Monologue |
| Παρωδία | Parody |
| Ιστορικό | Historical |
| Βιογραφία | Biography |
| Αυτοσχεδιασμός | Improv |
| Διαδραστικό | Interactive |
| Performance | Performance |
| Επιθεώρηση | Revue |
| Magic Show | Magic Show |
| Άλλο | Other |

### Locations (`location`)

| Value | Region |
|---|---|
| Αττική | Attica (Athens) |
| Θεσσαλονίκη | Thessaloniki |
| Υπόλοιπη Ελλάδα | Rest of Greece |
| Κρήτη / Ηράκλειο | Crete / Heraklion |
| Αχαΐα | Achaea |
| Ιόνια / Κέρκυρα | Ionian / Corfu |
| Ήπειρος / Ιωάννινα | Epirus / Ioannina |
| Θεσσαλία / Λάρισα | Thessaly / Larissa |
| Δωδεκάνησα | Dodecanese |
| Κυκλάδες | Cyclades |
| Πανελλαδική Περιοδεία | Nationwide Tour |

> Full location list available in `ScraperService.cs` → `LocationMap`.

---

## Configuration

The default target URL is configured in `appsettings.json`:

```json
{
  "ScraperSettings": {
    "TargetUrl": "https://www.more.com/el/theater/"
  }
}
```

---

## Notes

- The scraper opens a **non-headless** browser by default (visible window). To run headlessly, set `Headless = true` in `ScraperService.cs`.
- Scraping time scales with the number of events found — each event detail page is opened in a new tab.
- The API handles cookie banners and region selection dialogs automatically.

---

## License

MIT
