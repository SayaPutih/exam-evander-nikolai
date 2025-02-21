using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Acceloka.API.Services;
using AccelokaDb.Entities.Models;
using Microsoft.Extensions.Logging;

namespace Acceloka.API.Controllers
{
    [Route("api/v1/tikets")]
    [ApiController]
    public class TiketController : ControllerBase
    {
        private readonly TicketService _ticketService;
        private readonly ILogger<TiketController> _logger;

        public TiketController(TicketService ticketService, ILogger<TiketController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        [HttpGet("get-available-ticket")]
        public async Task<IActionResult> GetAvailableTickets(
            [FromQuery] string? categoryName,
            [FromQuery] string? ticketCode,
            [FromQuery] string? ticketName,
            [FromQuery] decimal? maxPrice,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? orderBy = "kodeTiket",
            [FromQuery] string? orderState = "asc")
        {
            try
            {
                startDate ??= DateTime.SpecifyKind(DateTime.Parse("2023-11-05T19:00:00"), DateTimeKind.Utc);
                endDate ??= DateTime.SpecifyKind(DateTime.Parse("2023-11-05T19:00:00"), DateTimeKind.Utc);

                var tikets = await _ticketService.GetAvailableTickets(categoryName, ticketCode, ticketName, maxPrice, startDate, endDate, orderBy, orderState);
                return Ok(tikets);
            }
            catch (FormatException ex)
            {
                return Problem(
                    type: "https://tools.ietf.org/html/rfc7807",
                    title: "Invalid Date Format",
                    detail: "The date format provided is incorrect. Please use ISO 8601 format (YYYY-MM-DDTHH:MM:SSZ).",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (Exception ex)
            {
                return Problem(
                    type: "https://tools.ietf.org/html/rfc7807",
                    title: "Internal Server Error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }
    }
}