using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiCompare.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportController(SupportChatService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Ask(
            [FromBody] string question,
            CancellationToken ct)
        {
            var answer = await service.AskAsync(question, ct);
            return Ok(new { answer });
        }
    }
}
