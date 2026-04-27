using Microsoft.AspNetCore.Mvc;
using PlaywrightScraperAPI.Services;

namespace PlaywrightScraperAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScraperController : ControllerBase
{
    private readonly ScraperService _scraperService;

    public ScraperController(ScraperService scraperService)
    {
        _scraperService = scraperService;
    }

    [HttpPost("scrape")] // Χρησιμοποιούμε POST για να δεχτούμε Body
    public async Task<IActionResult> ScrapeUrl([FromBody] ScrapeRequest request)
    {
        if (request == null)
        {
            return BadRequest("Το σώμα της αίτησης (body) δεν μπορεί να είναι κενό.");
        }

        try
        {
            // Περνάμε ολόκληρο το αντικείμενο request που περιέχει τα location και category
            var result = await _scraperService.ScrapeWebsiteAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Σφάλμα κατά το scraping: {ex.Message}");
        }
    }
}