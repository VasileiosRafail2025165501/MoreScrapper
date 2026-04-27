using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

namespace PlaywrightScraperAPI.Services;

public class ScrapeRequest
{
    public string? Location { get; set; }
    public string? Category { get; set; }
}

public class ScraperService
{
    private readonly string _targetUrl;

    public ScraperService(IConfiguration configuration)
    {
        _targetUrl = configuration["ScraperSettings:TargetUrl"] ?? "https://www.more.com/el/theater/";
    }

    public async Task<ScrapeResult> ScrapeWebsiteAsync(ScrapeRequest request)
    {
        // Χαρτογράφηση κειμένου σε καθαρές CSS κλάσεις (όπως φαίνονται στο τέλος του class attribute στο HTML σου)
        var categoryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Δικαστικό θρίλερ", "theatercourtthriller" },
            { "Κοινωνικό Δράμα", "theatersocialdrama" },
            { "Κοινωνικό", "theatersocialdrama" },
            { "Δράμα", "theaterdrama" },
            { "Κωμωδία", "theatercomedy" },
            { "Παιδικά", "theaterforkids" },
            { "Άλλο", "theaterother" }
        };

        var locationMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Αττική", "area1" },
            { "Θεσσαλονίκη", "area1060" },
            { "Αχαΐα", "area1012" }
        };

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false 
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync(_targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // 1. Cookies
        var rejectBtn = page.Locator("a.cc-btn--reject");
        if (await rejectBtn.IsVisibleAsync()) await rejectBtn.ClickAsync();

        // 2. Country Selector
        var greeceLink = page.Locator("#PageContent_CSel_GR_Select");
        if (await greeceLink.IsVisibleAsync())
        {
            await greeceLink.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Επιστροφή στο Θέατρο αν έγινε ανακατεύθυνση στην αρχική
            var theaterMenu = page.Locator("#NavBar_rptNavigation_listcontainer_2 a");
            if (await theaterMenu.IsVisibleAsync()) await theaterMenu.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // 3. Επιλογή Κατηγορίας από Dropdown
        if (!string.IsNullOrEmpty(request.Category))
        {
            await page.Locator("a.genreDropDown").First.ClickAsync();
            var searchInput = page.GetByPlaceholder("Αναζήτηση κατηγορίας");
            await searchInput.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await searchInput.FillAsync(request.Category);
            await Task.Delay(800); // Χρόνος για το UI filter

            var categoryOption = page.Locator("#genre ul.mainGenres li a")
                .GetByText(request.Category, new() { Exact = true });

            if (await categoryOption.CountAsync() > 0)
            {
                await categoryOption.First.ClickAsync(new() { Force = true });
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }
        }

        // 4. Επιλογή Τοποθεσίας από Dropdown
        if (!string.IsNullOrEmpty(request.Location))
        {
            await page.Locator("a.locationDropDown").First.ClickAsync();
            var regionsBtn = page.Locator("a[href='#location-cities']").Filter(new() { HasText = "Περιοχές" });
            await regionsBtn.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await regionsBtn.ClickAsync();

            var cityOptions = page.Locator("#location-cities ul.mm-listview a")
                .GetByText(request.Location, new() { Exact = true });

            int cityCount = await cityOptions.CountAsync();
            for (int i = 0; i < cityCount; i++)
            {
                await cityOptions.Nth(i).ClickAsync(new() { Force = true });
                await Task.Delay(400); 
            }
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // 5. ΦΙΛΤΡΑΡΙΣΜΑ ΚΑΙ ΑΝΟΙΓΜΑ ΤΑΒS
        // Περιμένουμε το JavaScript να κρύψει ό,τι δεν ταιριάζει
        await Task.Delay(3000);

        string genreCls = categoryMap.GetValueOrDefault(request.Category ?? "", "");
        string areaCls = locationMap.GetValueOrDefault(request.Location ?? "", "");

        // Χτίζουμε τον selector: 
        // article:visible -> μόνο όσα είναι ορατά μετά το φιλτράρισμα
        string finalSelector = "article:visible";
        if (!string.IsNullOrEmpty(genreCls)) finalSelector += $".{genreCls}";
        if (!string.IsNullOrEmpty(areaCls)) finalSelector += $".{areaCls}";
        finalSelector += " a#ItemLink";

        Console.WriteLine($"Executing search with Selector: {finalSelector}");

        var filteredEvents = page.Locator(finalSelector);
        int totalFound = await filteredEvents.CountAsync();
        Console.WriteLine($"Found {totalFound} results.");

        for (int i = 0; i < totalFound; i++)
        {
            var href = await filteredEvents.Nth(i).GetAttributeAsync("href");
            if (string.IsNullOrEmpty(href)) continue;

            string fullUrl = href.StartsWith("http") ? href : $"https://www.more.com{href}";
            
            Console.WriteLine($"Opening Tab {i+1}: {fullUrl}");

            var newTab = await browser.NewPageAsync();
            try 
            {
                await newTab.GotoAsync(fullUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                
                // Παράδειγμα scraping μέσα στο event
                string title = await newTab.TitleAsync();
                Console.WriteLine($"Scraped Title: {title}");
                
                await newTab.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening {fullUrl}: {ex.Message}");
                await newTab.CloseAsync();
            }
        }

        // 6. Τελική καθυστέρηση για έλεγχο
        await Task.Delay(5000);
        return new ScrapeResult { Url = page.Url, Title = "Scraping Finished" };
    }
}

public record ScrapeResult
{
    public string Url { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ContentSnippet { get; init; } = string.Empty;
}