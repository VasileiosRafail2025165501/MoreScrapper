# 🎭 MoreScrapperAPI
**MoreScrapperAPI** is a .NET 8 Web API that uses Microsoft Playwright to dynamically scrape theater event data from more.com. It accepts specific search criteria (like category and location), navigates the website, handles cookies and UI interactions, and returns a structured JSON response containing detailed information about the events.
## ✨ Features
 * **Dynamic Web Scraping:** Uses a real Chromium browser via Playwright to ensure all dynamic JavaScript content is fully loaded.
 * **Automated Navigation:** Automatically accepts cookies, selects the correct country/region, and applies user-defined filters (Category & Location).
 * **Deep Extraction:** Opens individual event tabs to extract:
   * Title
   * Event URL & Image URL
   * Location / Venue Name
   * Detailed Description (About)
   * Date range
   * Duration
   * Google Maps Link
 * **Swagger UI:** Built-in Swagger interface for easy API testing and exploration.
## 🛠️ Tech Stack
 * **Framework:** C# / .NET 8.0
 * **Scraping Engine:** Microsoft Playwright
 * **API Documentation:** Swashbuckle (Swagger)
## 🚀 Getting Started
### Prerequisites
 1. .NET 8.0 SDK installed on your machine.
 2. PowerShell (required to install Playwright browser binaries).
### Installation
 1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/MoreScrapperAPI.git
   cd MoreScrapperAPI
   
   ```
 2. **Build the project:**
   ```bash
   dotnet build
   
   ```
 3. **Install Playwright Browsers:**
   Because Playwright requires specific browser binaries to run, you must install them after your first build:
   ```bash
   pwsh bin/Debug/net8.0/playwright.ps1 install
   
   ```
 4. **Run the API:**
   ```bash
   dotnet run
   
   ```
## 📖 API Usage
Once the application is running, open your browser and navigate to http://localhost:<port>/swagger to access the Swagger UI.
### Endpoint: POST /api/scraper/scrape
**Request Body (ScrapeRequest):**
```json
{
  "location": "Θεσσαλονίκη",
  "category": "Κωμωδία"
}

```
*(Leave strings empty "" to scrape all available categories/locations).*
**Success Response (200 OK):**
```json
{
  "url": "https://www.more.com/el/theater/",
  "message": "Επιτυχής εξαγωγή 1 παραστάσεων.",
  "totalEventsScraped": 1,
  "events": [
    {
      "title": "Όλα Μόνοι Μας στη Θεσσαλονίκη",
      "url": "https://www.more.com/gr-el/tickets/theater/ola-monoi-mas-1/",
      "imageUrl": "https://www.more.com/getattachment/.../image.png",
      "locationName": "Θέατρο Τεχνών Θεσσαλονίκης, Κωνσταντινουπόλεως 75",
      "about": "Μετά από δύο μαγικές FULL HOUSE βραδιές μες στο Μάρτη...",
      "date": "28 Οκτ - 20 Δεκ 2026",
      "duration": "90'",
      "mapUrl": "https://maps.google.com/maps?ll=38.010683,23.735833&z=15&t=m&hl=en&gl=GR&mapclient=embed&cid=13177469685007371004"
    }
  ]
}

```
## 📁 Project Structure
 * Controllers/ScraperController.cs: The HTTP entry point exposing the POST endpoint.
 * Services/ScraperService.cs: The core engine containing all Playwright automation, DOM traversal, and scraping logic.
 * Program.cs: Setup for Dependency Injection, Swagger, and App configuration.
## ⚠️ Disclaimer
This project is intended for educational purposes. Web scraping should be done responsibly and in accordance with the target website's Terms of Service and robots.txt. The developers assume no liability for misuse of this software.
## 📄 License
This project is licensed under the MIT License - see the LICENSE file for details.
*** 
