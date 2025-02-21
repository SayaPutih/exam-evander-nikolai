using System;
using System.Collections.Generic;

namespace AccelokaDb.Entities.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string NamaKategori { get; set; } = null!;
        public string KodeTiket { get; set; } = null!;
        public string NamaTiket { get; set; } = null!;
        public DateTime TanggalEvent { get; set; }
        public decimal Harga { get; set; }
        public int SisaQuota { get; set; }
        public virtual ICollection<BookedTicket> BookedTickets { get; set; } = new List<BookedTicket>();
    }
}
