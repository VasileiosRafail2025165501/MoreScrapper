using Microsoft.Playwright;

namespace PlaywrightScraperAPI.Services;

public class ScrapeRequest
{
    public string? Location { get; set; }
    public string? Category { get; set; }
}

public class ScraperService
{
    private readonly string _targetUrl;

   private static readonly Dictionary<string, string> CategoryMap = new(StringComparer.OrdinalIgnoreCase)
{
    // Main 
    { "Δράμα", "theaterdrama" },
    { "Κωμωδία", "theatercomedy" },
    { "Μουσικό", "theatermusical" },
    { "Μουσικό Θέατρο", "theatermusictheater" },
    { "Παιδικά", "theaterforkids" },
    { "Άλλο", "theaterother" },
    { "Αρχαίο Δράμα", "theaterancientdrama" },
    { "Τραγωδία", "theatertragedy" },
    { "Κλασικό", "theaterclassical" },
    { "Κοινωνικό Δράμα", "theatersocialdrama" },
    { "Κοινωνικό", "theatersocialdrama" },
    { "Δικαστικό Θρίλερ", "theatercourtthriller" },
    { "Μυστήριο", "theatermystery" },
    { "Μαύρη Κωμωδία", "theaterblackcomedy" },
    { "Μονόλογος", "theatermonologue" },
    { "Παρωδία", "theaterparody" },
    { "Ιστορικό", "theaterhistory" },
    { "Βιογραφία", "theaterbiography" },
    { "Μυθιστόρημα", "theaternovel" },
    { "Αυτοσχεδιασμός", "theaterimprov" },
    { "Διαδραστικό", "theaterinteractive" },
    { "Εμπειρικό", "theaterexperiential" },
    { "Performance", "theaterperformance" },
    { "Επιθεώρηση", "theaterrevue" },
    { "Θέατρο Κούκλας/Σκιών", "theatershadow" },
    { "Ακροβατικό", "theaterstunts" },
    { "Magic Show", "theatermagicshow" },
};

private static readonly Dictionary<string, string> LocationMap = new(StringComparer.OrdinalIgnoreCase)
{
    // Single-region codes (clear 1-to-1 mapping)
    { "Αττική", "area1" },
    { "Υπόλοιπη Ελλάδα", "area2" },
    { "Αργολίδα", "area1009" },

    // Multi-region tour circuits — mapped by their primary/dominant region
    { "Θεσσαλονίκη", "area1060" },
    { "Αχαΐα", "area1012" },
    { "Κρήτη / Ηράκλειο", "area1016" },
    { "Ιόνια / Κέρκυρα", "area1038" },
    { "Μακεδονία / Πέλλα", "area1008" },
    { "Ήπειρος / Ιωάννινα", "area1030" },
    { "Θεσσαλία / Λάρισα", "area1010" },
    { "Στερεά Ελλάδα / Βοιωτία", "area1011" },
    { "Δυτική Ελλάδα / Αιτωλοακαρνανία", "area1014" },
    { "Πελοπόννησος / Κόρινθος", "area1015" },
    { "Δωδεκάνησα", "area1017" },
    { "Κυκλάδες", "area1019" },
    { "Βόρεια Ελλάδα / Καβάλα", "area1021" },
    { "Ανατολική Μακεδονία / Ξάνθη", "area1022" },
    { "Κεντρική Μακεδονία / Ημαθία", "area1023" },
    { "Δυτική Μακεδονία / Κοζάνη", "area1024" },
    { "Εύβοια", "area1025" },
    { "Λέσβος / Αιγαίο", "area1027" },
    { "Κρήτη / Χανιά", "area1028" },
    { "Ζάκυνθος / Κεφαλονιά", "area1029" },
    { "Θεσπρωτία / Πρέβεζα", "area1031" },
    { "Λακωνία / Μεσσηνία", "area1032" },
    { "Κεντρική Μακεδονία / Σέρρες", "area1033" },
    { "Δράμα / Καβάλα", "area1034" },
    { "Πανελλαδική Περιοδεία", "area1036" },
    { "Κρήτη / Ρέθυμνο", "area1037" },
    { "Νησιά Αιγαίου", "area1039" },
    { "Θεσσαλία / Τρίκαλα", "area1040" },
    { "Ιόνια / Λευκάδα", "area1041" },
    { "Πελοπόννησος / Μεσσηνία", "area1042" },
    { "Κρήτη / Λασίθι", "area1043" },
    { "Βόρεια Ελλάδα / Γρεβενά", "area1044" },
    { "Αχαΐα / Ηλεία", "area1045" },
    { "Μακεδονία / Πιερία", "area1046" },
    { "Κρήτη / Χανιά (Β)", "area1047" },
    { "Φωκίδα / Εύρυτανία", "area1048" },
    { "Κεντρική Μακεδονία / Καστοριά", "area1049" },
    { "Θεσσαλία / Μαγνησία", "area1052" },
    { "Πελοπόννησος / Λακωνία", "area1053" },
    { "Βόρεια Ελλάδα / Έβρος", "area1055" },
    { "Ήπειρος / Άρτα", "area1056" },
    { "Δυτική Μακεδονία / Δράμα", "area1057" },
    { "Κρήτη / Ηράκλειο (Β)", "area1059" },
    { "Κεντρική Μακεδονία / Χαλκιδική", "area1069" },
};

    public ScraperService(IConfiguration configuration)
    {
        _targetUrl = configuration["ScraperSettings:TargetUrl"] ?? "https://www.more.com/el/theater/";
    }

    public async Task<ScrapeResult> ScrapeWebsiteAsync(ScrapeRequest request)
    {
        // --- Build dynamic selector from request ---
        string categoryClass =
            (!string.IsNullOrEmpty(request.Category) && CategoryMap.TryGetValue(request.Category, out var cat))
                ? $".{cat}"
                : string.Empty;

        string locationClass =
            (!string.IsNullOrEmpty(request.Location) && LocationMap.TryGetValue(request.Location, out var loc))
                ? $".{loc}"
                : string.Empty;

        // e.g. "article:visible.theatercourtthriller.area1 a#ItemLink"
        string finalSelector = $"article:visible{categoryClass}{locationClass} a#ItemLink";
        Console.WriteLine($"Using selector: {finalSelector}");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync(_targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // 1. Reject cookies
        var rejectBtn = page.Locator("a.cc-btn--reject");
        if (await rejectBtn.IsVisibleAsync()) await rejectBtn.ClickAsync();

        // 2. Country selector
        var greeceLink = page.Locator("#PageContent_CSel_GR_Select");
        if (await greeceLink.IsVisibleAsync())
        {
            await greeceLink.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var theaterMenu = page.Locator("#NavBar_rptNavigation_listcontainer_2 a");
            if (await theaterMenu.IsVisibleAsync()) await theaterMenu.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        await Task.Delay(3000);

        // 3. Επιλογή Κατηγορίας από Dropdown
        if (!string.IsNullOrEmpty(request.Category))
        {
            await page.Locator("a.genreDropDown").First.ClickAsync();

            await Task.Delay(800); // Χρόνος για το UI filter

            var categoryOption = page.Locator("#genre ul.mainGenres li a")
                .GetByText(request.Category, new() { Exact = true });

            if (await categoryOption.CountAsync() > 0)
            {
                await categoryOption.First.ClickAsync(new() { Force = true });
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }
        }

        await Task.Delay(3000);

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


        await Task.Delay(3000);

        string categoryClassName = categoryClass.TrimStart('.'); // e.g. "theatercourtthriller"
        string locationClassName = locationClass.TrimStart('.'); // e.g. "area1"

        var allArticles = page.Locator("article");
        int total = await allArticles.CountAsync();
        Console.WriteLine($"[INFO] Total articles in DOM: {total}");

        var urls = new List<string>();

        for (int i = 0; i < total; i++)
        {
            var article = allArticles.Nth(i);
            string cls = await article.GetAttributeAsync("class") ?? "";
            var classes = new HashSet<string>(cls.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            if (!string.IsNullOrEmpty(categoryClassName))
            {
                bool hasCategory = classes.Any(c => c.StartsWith(categoryClassName));
                if (!hasCategory) continue;
            }

            if (!string.IsNullOrEmpty(locationClassName))
            {
                bool hasLocation = classes.Any(c =>
                    c.StartsWith(locationClassName) &&
                    (c.Length == locationClassName.Length ||
                     c[locationClassName.Length] == 'd'));
                if (!hasLocation) continue;
            }

            var link = article.Locator("a#ItemLink");
            if (await link.CountAsync() == 0) continue;

            var href = await link.First.GetAttributeAsync("href");
            if (string.IsNullOrEmpty(href)) continue;

            string fullUrl = href.StartsWith("http") ? href : $"https://www.more.com{href}";
            urls.Add(fullUrl);
            Console.WriteLine($"[MATCH] {fullUrl}");
        }

        Console.WriteLine($"--- Found {urls.Count} matching events ---");

        if (urls.Count == 0)
        {
            Console.WriteLine("[INFO] No results found.");
            return new ScrapeResult { Url = page.Url, Title = "No results found" };
        }

        foreach (var url in urls)
        {
            Console.WriteLine($"Opening: {url}");
            var newTab = await browser.NewPageAsync();
            try
            {
                await newTab.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                string eventTitle = await newTab.TitleAsync();
                Console.WriteLine($"Opened: {eventTitle}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {url}: {ex.Message}");
                await newTab.CloseAsync();
            }
        }

        return new ScrapeResult
        {
            Url = page.Url,
            Title = $"Opened {urls.Count} theater(s)"
        };
    }

    public record ScrapeResult
    {
        public string Url { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string ContentSnippet { get; init; } = string.Empty;
    }
}