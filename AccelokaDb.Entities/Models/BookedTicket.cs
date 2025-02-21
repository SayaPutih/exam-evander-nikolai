using System;
using System.Collections.Generic;

namespace AccelokaDb.Entities.Models;

public partial class BookedTicket
{
    public int Id { get; set; }

    public string KodeTiket { get; set; } = null!;

    public int Quantity { get; set; }

    public DateTime? BookedDate { get; set; }

    public virtual Tiket KodeTiketNavigation { get; set; } = null!;
}
