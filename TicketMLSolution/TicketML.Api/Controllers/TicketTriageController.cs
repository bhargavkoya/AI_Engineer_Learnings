using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using TicketML.Api.Models;

namespace TicketML.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketTriageController : ControllerBase
    {
        private readonly PredictionEnginePool<TicketRecordInput, TicketPredictionOutput> _pool;

        // Constructor injection makes the DI wiring explicit - no static state,
        // no ambiguity about lifetime, which is why a controller fits better
        // here than a minimal API's inline lambda for this dependency.
        public TicketTriageController(PredictionEnginePool<TicketRecordInput, TicketPredictionOutput> pool)
        {
            _pool = pool;
        }

        [HttpPost("predict")]
        public ActionResult<TicketPredictionOutput> Predict([FromBody] TicketRecordInput input)
        {
            if (input is null) return BadRequest("Ticket data is required.");

            // Pooled engine - safe under concurrent requests, unlike a shared
            // singleton PredictionEngine.
            var prediction = _pool.Predict(modelName: "TicketTriage", example: input);
            return Ok(prediction);
        }
    }
}
