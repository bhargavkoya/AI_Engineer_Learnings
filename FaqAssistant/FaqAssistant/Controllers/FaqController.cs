using FaqAssistant.Models;
using FaqAssistant.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FaqAssistant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaqController(
    FaqVectorService faqService,
    FaqAssistantService assistantService) : ControllerBase
    {
        // POST api/faq/seed - upload a batch of FAQ entries
        [HttpPost("seed")]
        public async Task<IActionResult> SeedAsync(
            [FromBody] List<FaqEntry> entries, CancellationToken ct)
        {
            await faqService.EnsureCollectionAsync(ct);

            foreach (var entry in entries)
            {
                await faqService.UpsertFaqAsync(entry, ct);
            }

            return Ok(new { seeded = entries.Count });
        }

        // GET api/faq/ask?q=how+do+I+deploy&category=devops
        [HttpGet("ask")]
        public async Task<IActionResult> AskAsync(
            [FromQuery] string q,
            [FromQuery] string? category,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query parameter 'q' is required.");

            var answer = await assistantService.AskAsync(q, category, ct);
            return Ok(new { question = q, answer });
        }
    }
}
