using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Acceloka.API.Models
{
    public class BookedTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string KodeTiket { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime BookedDate { get; set; }
    }
}