using Microsoft.AspNetCore.Mvc;

namespace AiCompare.Api
{
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
