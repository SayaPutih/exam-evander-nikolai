using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Acceloka.API.Models
{
    public class BookTicketRequest
    {
        [Required]
        public List<TicketBooking> Tickets { get; set; }
    }

    public class TicketBooking
    {
        [Required]
        public string KodeTiket { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity harus lebih dari 0")]
        public int Quantity { get; set; }
    }
}
