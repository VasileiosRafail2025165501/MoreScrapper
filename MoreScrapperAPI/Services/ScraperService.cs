using Microsoft.Playwright;

namespace PlaywrightScraperAPI.Services;

public class ScraperService
{
    public async Task<ScrapeResult> ScrapeWebsiteAsync(string url)
    {
        // Αρχικοποίηση του Playwright
        using var playwright = await Playwright.CreateAsync();
        
        // Εκκίνηση του Chromium σε Headless mode (στο παρασκήνιο, χωρίς UI)
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
        { 
            Headless = true 
        });
        
        var page = await browser.NewPageAsync();
        
        // Πλοήγηση στο URL (περιμένουμε να φορτώσει)
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        
        // Τράβηγμα δεδομένων (Scraping)
        var title = await page.TitleAsync();
        var content = await page.Locator("body").InnerTextAsync(); // Παίρνει όλο το ορατό κείμενο του body
        
        return new ScrapeResult
        {
            Url = url,
            Title = title,
            ContentSnippet = content.Length > 200 ? content.Substring(0, 200) + "..." : content
        };
    }
}

public record ScrapeResult
{
    public string Url { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ContentSnippet { get; init; } = string.Empty;
}