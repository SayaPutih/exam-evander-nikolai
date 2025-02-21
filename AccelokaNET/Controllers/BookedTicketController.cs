using System.Threading.Tasks;
using Acceloka.API.Models;
using Acceloka.API.Services;
using AccelokaNET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Acceloka.API.Controllers
{
    [ApiController]
    [Route("api/v1/book-ticket")]
    public class BookedTicketController : ControllerBase
    {
        private readonly BookedTicketService _bookedTicketService;
        private readonly ILogger<BookedTicketController> _logger; 

        public BookedTicketController(BookedTicketService bookedTicketService, ILogger<BookedTicketController> logger)
        {
            _bookedTicketService = bookedTicketService;
            _logger = logger; 
        }

        [HttpPost]
        public async Task<IActionResult> BookTickets([FromBody] BookTicketRequest request)
        {
            if (request == null || request.Tickets == null || request.Tickets.Count == 0)
            {
                _logger.LogWarning("Invalid request data received for booking tickets."); 
                return BadRequest(new { error = "Invalid request data" });
            }

            _logger.LogInformation("Booking tickets: {@Tickets}", request.Tickets);

            var bookingRequests = request.Tickets.Select(t => (t.KodeTiket, t.Quantity)).ToList();
            var result = await _bookedTicketService.BookTicketsAsync(bookingRequests);

            if (result is IDictionary<string, object> errorResponse && errorResponse.ContainsKey("error"))
            {
                _logger.LogWarning("Error booking tickets: {@ErrorResponse}", errorResponse); 
                return BadRequest(errorResponse);
            }

            _logger.LogInformation("Successfully booked tickets: {@Result}", result);
            return Ok(result);
        }

        [HttpGet("get-booked-ticket/{bookedTicketId}")]
        public async Task<IActionResult> GetBookedTicketDetails(int bookedTicketId)
        {
            var result = await _bookedTicketService.GetBookedTicketDetailsAsync(bookedTicketId);

            if (result == null)
            {
                _logger.LogWarning("BookedTicketId not found: {BookedTicketId}", bookedTicketId);
                return NotFound(new
                {
                    type = "https://httpstatuses.com/404",
                    title = "Not Found",
                    status = 404,
                    detail = "BookedTicketId tidak terdaftar"
                });
            }

            _logger.LogInformation("Details retrieved for BookedTicketId: {BookedTicketId}", bookedTicketId);
            return Ok(result);
        }

        [HttpDelete("revoke-ticket/{bookedTicketId}/{kodeTiket}/{qty}")]
        public async Task<IActionResult> RevokeTicket(int bookedTicketId, string kodeTiket, int qty)
        {
            var result = await _bookedTicketService.RevokeTicketAsync(bookedTicketId, kodeTiket, qty);

            if (result is IDictionary<string, object> errorResponse && errorResponse.ContainsKey("error"))
            {
                _logger.LogWarning("Error revoking ticket: {@ErrorResponse}", errorResponse);
                return BadRequest(errorResponse);
            }

            _logger.LogInformation("Successfully revoked ticket: {KodeTiket}, Quantity: {Qty}", kodeTiket, qty);
            return Ok(result);
        }

        [HttpPut("edit-booked-ticket/{bookedTicketId}")]
        public async Task<IActionResult> EditBookedTickets(int bookedTicketId, [FromBody] List<TicketBookingModel> request)
        {
            if (request == null || request.Count == 0)
            {
                _logger.LogWarning("Invalid request data received for editing booked tickets."); 
                return BadRequest(new { error = "Invalid request data" });
            }

            var updatedTickets = request.Select(t => (t.KodeTiket, t.Quantity)).ToList();
            var result = await _bookedTicketService.EditBookedTicketsAsync(bookedTicketId, updatedTickets);

            if (result is IDictionary<string, object> errorResponse && errorResponse.ContainsKey("error"))
            {
                _logger.LogWarning("Error editing booked tickets: {@ErrorResponse}", errorResponse); 
                return BadRequest(errorResponse);
            }

            _logger.LogInformation("Successfully edited booked tickets for BookedTicketId: {BookedTicketId}", bookedTicketId); 
            return Ok(result);
        }
    }
}