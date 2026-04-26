using Microsoft.AspNetCore.Mvc;
using PlaywrightScraperAPI.Services;

namespace PlaywrightScraperAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScraperController : ControllerBase
{
    private readonly ScraperService _scraperService;

    // Dependency Injection του service
    public ScraperController(ScraperService scraperService)
    {
        _scraperService = scraperService;
    }
//testg
    [HttpGet("scrape")]
    public async Task<IActionResult> ScrapeUrl([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest("Το URL είναι υποχρεωτικό.");
        }

        try
        {
            var result = await _scraperService.ScrapeWebsiteAsync(url);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Προσοχή: Σε production δεν γυρνάμε το ex.Message απευθείας, αλλά για development βοηθάει
            return StatusCode(500, $"Σφάλμα κατά το scraping: {ex.Message}");
        }
    }
}