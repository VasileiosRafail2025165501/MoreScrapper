using Microsoft.Playwright;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace PlaywrightScraperAPI.Services;

public class ScrapeRequest
{
    public string? Location { get; set; }
    public string? Category { get; set; }
}

public record Showtime
{
    public string Date { get; init; } = string.Empty;
    public string Time { get; init; } = string.Empty;
    public string Availability { get; init; } = string.Empty;
    public string VenueName { get; init; } = string.Empty;
}

public record EventDetail
{
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string ImageBase64 { get; init; } = string.Empty;
    public string LocationName { get; init; } = string.Empty;
    public string About { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public string MapUrl { get; init; } = string.Empty;
    public string Coordinates { get; init; } = string.Empty;
    public List<Showtime> Showtimes { get; init; } = new();
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

    // Max parallel tabs — tune this up/down based on your machine
    private const int MaxParallelTabs = 8;

    // Strips embedded base64 data URIs from description text
    private static readonly Regex Base64DataUriRegex = new Regex(
        @"data:[a-zA-Z0-9+/]+;base64,[A-Za-z0-9+/=]+",
        RegexOptions.Compiled
    );

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
        { "Κλασικό έργο", "theaterclassical" },
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

        Console.WriteLine($"Using selector: article:visible{categoryClass}{locationClass} a#ItemLink");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        var page = await browser.NewPageAsync();

        await page.GotoAsync(_targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        var rejectBtn = page.Locator("a.cc-btn--reject");
        if (await rejectBtn.IsVisibleAsync()) await rejectBtn.ClickAsync();

        var greeceLink = page.Locator("#PageContent_CSel_GR_Select");
        if (await greeceLink.IsVisibleAsync())
        {
            await greeceLink.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var theaterMenu = page.Locator("#NavBar_rptNavigation_listcontainer_2 a");
            if (await theaterMenu.IsVisibleAsync()) await theaterMenu.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
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

        Console.WriteLine($"--- Found {urls.Count} matching events. Starting parallel scrape (max {MaxParallelTabs} concurrent tabs) ---");

        if (urls.Count == 0)
        {
            Console.WriteLine("[INFO] No results found.");
            return new ScrapeResult { Url = page.Url, Message = "No results found", TotalEventsScraped = 0 };
        }

        // Thread-safe collection for results
        var results = new ConcurrentBag<(int Index, EventDetail Detail)>();

        // Semaphore limits how many tabs are open/processing at the same time
        using var semaphore = new SemaphoreSlim(MaxParallelTabs);

        var tasks = urls.Select((url, index) => Task.Run(async () =>
        {
            await semaphore.WaitAsync();
            var newTab = await browser.NewPageAsync();
            try
            {
                Console.WriteLine($"[{index + 1}/{urls.Count}] Opening: {url}");

                await newTab.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

                var newRejectBtn = newTab.Locator("a.cc-btn--reject");
                if (await newRejectBtn.IsVisibleAsync()) await newRejectBtn.ClickAsync();

                try
                {
                    await newTab.WaitForSelectorAsync(
                        "#r_descSummary, .r_mainInfoText, .r_descriptionText",
                        new PageWaitForSelectorOptions { Timeout = 5000, State = WaitForSelectorState.Attached }
                    );
                }
                catch { }

                // 1. TITLE
                var titleLoc = newTab.Locator("#r_maininfo h1, h1");
                string eventTitle = string.Empty;
                if (await titleLoc.CountAsync() > 0)
                {
                    eventTitle = await titleLoc.First.InnerTextAsync();
                }
                else
                {
                    eventTitle = await newTab.TitleAsync();
                    eventTitle = eventTitle.Split('|')[0];
                }

                // 2. IMAGE
                var imgLoc = newTab.Locator(".r_banner_img_container img[src], .r_banner picture img[src], .header-image img[src]");
                string srcValue = string.Empty;
                if (await imgLoc.CountAsync() > 0)
                {
                    srcValue = await imgLoc.First.GetAttributeAsync("src") ?? "";
                }
                else
                {
                    var sourceLoc = newTab.Locator(".r_banner_img_container picture source[srcset]");
                    if (await sourceLoc.CountAsync() > 0)
                    {
                        srcValue = await sourceLoc.First.GetAttributeAsync("srcset") ?? "";
                        srcValue = srcValue.Split(' ')[0];
                    }
                }
                string imageUrl = !string.IsNullOrEmpty(srcValue)
                    ? (srcValue.StartsWith("http") ? srcValue : $"https://www.more.com{srcValue}")
                    : "";

                string imageBase64 = string.Empty;
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    try
                    {
                        byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                        imageBase64 = Convert.ToBase64String(imageBytes);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARNING] Failed to download image at {url}: {ex.Message}");
                    }
                }

                // 3. LOCATION
                var locationLoc = newTab.Locator(".r_mainInfoLocation .r_mainInfoText");
                string locationStr = string.Empty;
                if (await locationLoc.CountAsync() > 0)
                {
                    locationStr = await locationLoc.First.InnerTextAsync() ?? "";
                    if (string.IsNullOrWhiteSpace(locationStr))
                        locationStr = await locationLoc.First.TextContentAsync() ?? "";
                }

                // CLICK "SEE MORE" IF EXISTS
                var viewMoreBtn = newTab.Locator(".r_viewMoreButton");
                if (await viewMoreBtn.IsVisibleAsync())
                {
                    try { await viewMoreBtn.ClickAsync(); await Task.Delay(500); } catch { }
                }

                // 4. ABOUT (DESCRIPTION)
                string[] aboutSelectors = new[]
                {
                    "#r_descSummary",
                    "#r_summaryText",
                    ".r_descriptionCustomText",
                    ".r_descriptionExpanded",
                    ".r_descriptionText",
                    ".r_description",
                    ".description-text"
                };
                string aboutStr = string.Empty;

                foreach (var selector in aboutSelectors)
                {
                    var loc = newTab.Locator(selector);
                    if (await loc.CountAsync() > 0)
                    {
                        aboutStr = await loc.First.InnerTextAsync() ?? "";
                        if (string.IsNullOrWhiteSpace(aboutStr))
                            aboutStr = await loc.First.TextContentAsync() ?? "";
                        if (!string.IsNullOrWhiteSpace(aboutStr)) break;
                    }
                }

                aboutStr = aboutStr.Replace("Δες λιγότερα", "").Trim();
                aboutStr = Base64DataUriRegex.Replace(aboutStr, "").Trim();
                int castIndex = aboutStr.IndexOf("Συντελεστές", StringComparison.OrdinalIgnoreCase);
                if (castIndex >= 0)
                    aboutStr = aboutStr.Substring(0, castIndex).Trim();

                // 5. DATE
                var dateLoc = newTab.Locator(".r_mainInfoDate .r_mainInfoText");
                string dateStr = await dateLoc.CountAsync() > 0
                    ? await dateLoc.First.InnerTextAsync()
                    : "";

                // 6. DURATION
                string durationStr = string.Empty;
                var durationWrapper = newTab
                    .Locator(".css-1v7fkxn")
                    .Filter(new LocatorFilterOptions { HasText = "Διάρκεια" });

                if (await durationWrapper.CountAsync() > 0)
                {
                    var durationText = durationWrapper.First.Locator("div p div, div p, .r_additionalText");
                    if (await durationText.CountAsync() > 0)
                        durationStr = (await durationText.First.InnerTextAsync()).Trim();
                }

                // 7. MAP URL AND COORDINATES
                var mapLoc = newTab.Locator(
                    "a[href*='maps.google'], a[title*='Χάρτες'], a[title*='Open in Maps'], a[aria-label*='Open in Maps']"
                );
                string mapUrl = string.Empty;
                string coordinates = string.Empty;

                if (await mapLoc.CountAsync() > 0)
                    mapUrl = await mapLoc.First.GetAttributeAsync("href") ?? "";

                if (string.IsNullOrEmpty(mapUrl))
                {
                    var inlineMapLoc = newTab.Locator(".events-container__item-map a[href*='maps']");
                    if (await inlineMapLoc.CountAsync() > 0)
                        mapUrl = await inlineMapLoc.First.GetAttributeAsync("href") ?? "";
                }

                if (string.IsNullOrEmpty(mapUrl))
                {
                    var googleMapsIframe = newTab.Locator("iframe.r_googleMaps[src*='maps']");
                    if (await googleMapsIframe.CountAsync() > 0)
                        mapUrl = await googleMapsIframe.First.GetAttributeAsync("src") ?? "";
                }

                if (string.IsNullOrEmpty(mapUrl))
                {
                    var iframeLoc = newTab.Locator("iframe[src*='maps']");
                    if (await iframeLoc.CountAsync() > 0)
                        mapUrl = await iframeLoc.First.GetAttributeAsync("src") ?? "";
                }

                if (!string.IsNullOrEmpty(mapUrl))
                {
                    var matchSimple = Regex.Match(mapUrl, @"(?:ll=|@)(-?\d+\.\d+)[,%](-?\d+\.\d+)");
                    if (matchSimple.Success)
                    {
                        coordinates = $"{matchSimple.Groups[1].Value}, {matchSimple.Groups[2].Value}";
                    }
                    else
                    {
                        var matchLon = Regex.Match(mapUrl, @"!2d(-?\d+\.\d+)");
                        var matchLat = Regex.Match(mapUrl, @"!3d(-?\d+\.\d+)");
                        if (matchLon.Success && matchLat.Success)
                            coordinates = $"{matchLat.Groups[1].Value}, {matchLon.Groups[1].Value}";
                    }
                }

                if (string.IsNullOrEmpty(coordinates))
                {
                    var bookBtn = newTab.Locator("a[vanuelat][vanuelon]");
                    if (await bookBtn.CountAsync() > 0)
                    {
                        string lat = await bookBtn.First.GetAttributeAsync("vanuelat") ?? "";
                        string lon = await bookBtn.First.GetAttributeAsync("vanuelon") ?? "";
                        if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
                            coordinates = $"{lat.Trim()}, {lon.Trim()}";
                    }
                }

                // 8. SHOWTIMES
                var extractedShowtimes = new List<Showtime>();
                var eventItems = newTab.Locator(".events-container__item");
                int itemsCount = await eventItems.CountAsync();

                for (int j = 0; j < itemsCount; j++)
                {
                    var item = eventItems.Nth(j);

                    var itemDateLoc = item.Locator(".events-container__item-date");
                    string showDate = await itemDateLoc.CountAsync() > 0
                        ? await itemDateLoc.First.InnerTextAsync()
                        : "";

                    var itemTimeLoc = item.Locator(".events-container__item-time");
                    string showTime = await itemTimeLoc.CountAsync() > 0
                        ? await itemTimeLoc.First.InnerTextAsync()
                        : "";

                    var bookBtnLoc = item.Locator("a[venuename]");
                    string venueName = await bookBtnLoc.CountAsync() > 0
                        ? await bookBtnLoc.First.GetAttributeAsync("venuename") ?? ""
                        : "";

                    if (string.IsNullOrEmpty(venueName))
                    {
                        var venueSpan = item.Locator(".events-container__item-venue");
                        if (await venueSpan.CountAsync() > 0)
                            venueName = (await venueSpan.First.InnerTextAsync()).Trim();
                    }

                    string classAttr = await item.GetAttributeAsync("class") ?? "";
                    string availability = "Unknown";

                    if (classAttr.Contains("eb-availability--green"))       availability = "Available";
                    else if (classAttr.Contains("eb-availability--orange"))  availability = "Limited";
                    else if (classAttr.Contains("eb-availability--red"))     availability = "Few Tickets Left";
                    else if (classAttr.Contains("eb-availability--soldout")) availability = "Sold Out";

                    if (!string.IsNullOrWhiteSpace(showDate))
                    {
                        extractedShowtimes.Add(new Showtime
                        {
                            Date = showDate.Trim(),
                            Time = showTime.Trim(),
                            Availability = availability,
                            VenueName = venueName.Trim()
                        });
                    }
                }

                var detail = new EventDetail
                {
                    Title = eventTitle.Trim(),
                    Url = url,
                    ImageUrl = imageUrl,
                    // ImageBase64 = imageBase64,
                    LocationName = locationStr.Trim(),
                    About = aboutStr.Trim(),
                    Date = dateStr.Trim(),
                    Duration = durationStr.Trim(),
                    MapUrl = mapUrl.Trim(),
                    Coordinates = coordinates,
                    Showtimes = extractedShowtimes
                };

                results.Add((index, detail));
                Console.WriteLine($"[{index + 1}/{urls.Count}] Done: {eventTitle.Trim()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] At URL {url}: {ex.Message}");
            }
            finally
            {
                await newTab.CloseAsync();
                semaphore.Release();
            }
        })).ToList();

        await Task.WhenAll(tasks);

        // Re-sort results to match original URL order (parallel execution scrambles order)
        var extractedEvents = results
            .OrderBy(r => r.Index)
            .Select(r => r.Detail)
            .ToList();

        return new ScrapeResult
        {
            Url = page.Url,
            Message = $"Successfully scraped {extractedEvents.Count} events.",
            TotalEventsScraped = extractedEvents.Count,
            Events = extractedEvents
        };
    }
}