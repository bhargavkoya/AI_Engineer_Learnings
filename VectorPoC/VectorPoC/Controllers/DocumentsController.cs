using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VectorPoC.Services;

namespace VectorPoC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController(DocumentService documentService) : ControllerBase
    {
        [HttpPost("index")]
        public async Task<IActionResult> Index(
            [FromBody] IndexRequest request,
            CancellationToken ct)
        {
            await documentService.IndexDocumentAsync(
                request.Title, request.Content, request.Category, ct);
            return Ok(new { message = "Document indexed." });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] string? category = null,
            [FromQuery] int topK = 5,
            CancellationToken ct = default)
        {
            var results = await documentService.SearchAsync(query, category, topK, ct);
            return Ok(results.Select(r => new
            {
                r.Id,
                r.Title,
                r.Category,
                // Don't return the raw embedding - it's ~6KB per result
                Preview = r.Content.Length > 200
                    ? r.Content[..200] + "..."
                    : r.Content
            }));
        }
    }

    public record IndexRequest(string Title, string Content, string Category);
}
