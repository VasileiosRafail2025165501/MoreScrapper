using Microsoft.Playwright;

namespace PlaywrightScraperAPI.Services;

public class ScrapeRequest
{
    public string? Location { get; set; }
    public string? Category { get; set; }
}

public record EventDetail
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public string About { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public string MapUrl { get; init; } = string.Empty;
    public string Coordinates { get; init; } = string.Empty;
}

public record ScrapeResult
{
    public string Url { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public int TotalEventsScraped { get; init; }
    public List<EventDetail> Events { get; init; } = new();
}

public class ScraperService
{
    private readonly string _targetUrl;
    
    private static readonly HttpClient _httpClient = new HttpClient();

    private static readonly Dictionary<string, string> CategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
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
        { "Αττική", "area1" },
        { "Υπόλοιπη Ελλάδα", "area2" },
        { "Αργολίδα", "area1009" },
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
        string categoryClass =
            (!string.IsNullOrEmpty(request.Category) && CategoryMap.TryGetValue(request.Category, out var cat))
                ? $".{cat}"
                : string.Empty;

        string locationClass =
            (!string.IsNullOrEmpty(request.Location) && LocationMap.TryGetValue(request.Location, out var loc))
                ? $".{loc}"
                : string.Empty;

        string finalSelector = $"article:visible{categoryClass}{locationClass} a#ItemLink";
        Console.WriteLine($"Using selector: {finalSelector}");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false 
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync(_targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var rejectBtn = page.Locator("a.cc-btn--reject");
        if (await rejectBtn.IsVisibleAsync()) await rejectBtn.ClickAsync();

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

        if (!string.IsNullOrEmpty(request.Category))
        {
            await page.Locator("a.genreDropDown").First.ClickAsync();
            await Task.Delay(800); 

            var categoryOption = page.Locator("#genre ul.mainGenres li a")
                .GetByText(request.Category, new() { Exact = true });

            if (await categoryOption.CountAsync() > 0)
            {
                await categoryOption.First.ClickAsync(new() { Force = true });
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }
        }

        await Task.Delay(4000);

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

        string categoryClassName = categoryClass.TrimStart('.'); 
        string locationClassName = locationClass.TrimStart('.'); 

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
            return new ScrapeResult { Url = page.Url, Message = "Δεν βρέθηκαν αποτελέσματα", TotalEventsScraped = 0 };
        }

        var extractedEvents = new List<EventDetail>();

        foreach (var url in urls)
        {
            Console.WriteLine($"Opening: {url}");
            var newTab = await browser.NewPageAsync();
            try
            {
                await newTab.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                
                var newrejectBtn = newTab.Locator("a.cc-btn--reject");
                if (await newrejectBtn.IsVisibleAsync()) await newrejectBtn.ClickAsync();

                await Task.Delay(1500); 
                
                var titleLoc = newTab.Locator("#r_maininfo h1");
                string eventTitle = await titleLoc.CountAsync() > 0 
                    ? await titleLoc.First.InnerTextAsync() 
                    : await newTab.TitleAsync();

                var imgLoc = newTab.Locator(".r_banner_img_container img");
                string srcValue = await imgLoc.CountAsync() > 0 
                    ? await imgLoc.First.GetAttributeAsync("src") ?? "" 
                    : "";
                    
                string imageUrl = !string.IsNullOrEmpty(srcValue) 
                    ? (srcValue.StartsWith("http") ? srcValue : $"https://www.more.com{srcValue}") 
                    : "";

                string imageBase64 = string.Empty;
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    try {
                        byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                        imageBase64 = Convert.ToBase64String(imageBytes);
                    } catch { }
                }

                var locationLoc = newTab.Locator(".r_mainInfoText, .venue-info"); 
                string locationStr = string.Empty;
                if (await locationLoc.CountAsync() > 0)
                {
                    locationStr = await locationLoc.First.InnerTextAsync() ?? "";
                    if (string.IsNullOrWhiteSpace(locationStr)) // Αν δεν πιάσει το InnerText, δοκιμάζουμε TextContent
                        locationStr = await locationLoc.First.TextContentAsync() ?? "";
                }
                
                var viewMoreBtn = newTab.Locator(".r_viewMoreButton");
                if (await viewMoreBtn.IsVisibleAsync())
                {
                    try {
                        await viewMoreBtn.ClickAsync();
                        await Task.Delay(1000); 
                    } catch { } 
                }

                var aboutLocator = newTab.Locator(".r_descriptionExpanded, .r_description, .r_descriptionText, .description-text, #info-tab-content, article"); 
                string aboutStr = string.Empty;
                if (await aboutLocator.CountAsync() > 0)
                {
                    aboutStr = await aboutLocator.First.InnerTextAsync() ?? "";
                    if (string.IsNullOrWhiteSpace(aboutStr))
                        aboutStr = await aboutLocator.First.TextContentAsync() ?? "";
                }
                
                var dateLoc = newTab.Locator(".r_mainInfoDate .r_mainInfoText");
                string dateStr = await dateLoc.CountAsync() > 0 
                    ? await dateLoc.First.InnerTextAsync() 
                    : "";

                var durationLoc = newTab.Locator(".r_additionalInfoTitle").Filter(new LocatorFilterOptions { HasText = "Διάρκεια" }).Locator("+ div");
                string durationStr = await durationLoc.CountAsync() > 0 
                    ? await durationLoc.First.InnerTextAsync() 
                    : "";

                var mapLoc = newTab.Locator("a[aria-label*='Open in Maps'], a[title*='Open in Maps'], a[href*='maps.google'], a:has-text('Χάρτης')");
                string mapUrl = string.Empty;
                string coordinates = string.Empty;

                if (await mapLoc.CountAsync() > 0)
                {
                    mapUrl = await mapLoc.First.GetAttributeAsync("href") ?? "";
                }
                else 
                {
                    var iframeLoc = newTab.Locator("iframe[src*='maps']");
                    if (await iframeLoc.CountAsync() > 0)
                    {
                        mapUrl = await iframeLoc.First.GetAttributeAsync("src") ?? "";
                    }
                }

                if (!string.IsNullOrEmpty(mapUrl))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(mapUrl, @"(-?\d+\.\d+)[,%](-?\d+\.\d+)");
                    if (match.Success)
                    {
                        coordinates = $"{match.Groups[1].Value}, {match.Groups[2].Value}";
                    }
                }

                extractedEvents.Add(new EventDetail
                {
                    Title = eventTitle.Trim(),
                    Url = url,
                    ImageUrl = imageUrl,
                    LocationName = locationStr.Trim(),
                    About = aboutStr.Trim(),
                    Date = dateStr.Trim(),          
                    Duration = durationStr.Trim(),  
                    MapUrl = mapUrl.Trim(),         
                    Coordinates = coordinates       
                });

                Console.WriteLine($"Επιτυχής εξαγωγή: {eventTitle}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Στο URL {url}: {ex.Message}");
            }
            finally
            {
                await newTab.CloseAsync(); 
            }
        }

        return new ScrapeResult
        {
            Url = page.Url,
            Message = $"Επιτυχής εξαγωγή {extractedEvents.Count} παραστάσεων.",
            TotalEventsScraped = extractedEvents.Count,
            Events = extractedEvents
        };
    }
}