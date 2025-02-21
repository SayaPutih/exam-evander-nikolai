using System.ComponentModel.DataAnnotations;

namespace AccelokaNET.Models
{
    public class TicketBookingModel
    {
        [Required]
        public string KodeTiket { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public int Quantity { get; set; }
    }
}
