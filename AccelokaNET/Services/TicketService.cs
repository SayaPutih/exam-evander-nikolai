using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AccelokaDb.Entities.Context;
using AccelokaDb.Entities.Models;
using Microsoft.Extensions.Logging;

namespace Acceloka.API.Services
{
    public class TicketService
    {
        private readonly AccelokaDbContext _context;
        private readonly ILogger<TicketService> _logger;

        public TicketService(AccelokaDbContext context, ILogger<TicketService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Tiket>> GetAvailableTickets(
            string? categoryName,
            string? ticketCode,
            string? ticketName,
            decimal? maxPrice,
            DateTime? startDate,
            DateTime? endDate,
            string orderBy,
            string orderState)
        {
            try
            {
                var query = _context.Tikets.Where(t => t.SisaQuota > 0);

                if (!string.IsNullOrEmpty(categoryName))
                    query = query.Where(t => t.NamaKategori.Contains(categoryName));

                if (!string.IsNullOrEmpty(ticketCode))
                    query = query.Where(t => t.KodeTiket.Contains(ticketCode));

                if (!string.IsNullOrEmpty(ticketName))
                    query = query.Where(t => t.NamaTiket.Contains(ticketName));

                if (maxPrice.HasValue)
                    query = query.Where(t => t.Harga <= maxPrice);

                if (startDate.HasValue)
                    query = query.Where(t => t.TanggalEvent >= startDate);

                if (endDate.HasValue)
                    query = query.Where(t => t.TanggalEvent <= endDate);

                if (string.IsNullOrEmpty(orderBy)) orderBy = "KodeTiket";
                if (string.IsNullOrEmpty(orderState)) orderState = "asc";

                query = orderBy.ToLower() switch
                {
                    "namakategori" => orderState.ToLower() == "desc" ? query.OrderByDescending(t => t.NamaKategori) : query.OrderBy(t => t.NamaKategori),
                    "kodetiket" => orderState.ToLower() == "desc" ? query.OrderByDescending(t => t.KodeTiket) : query.OrderBy(t => t.KodeTiket),
                    "namatiket" => orderState.ToLower() == "desc" ? query.OrderByDescending(t => t.NamaTiket) : query.OrderBy(t => t.NamaTiket),
                    "harga" => orderState.ToLower() == "desc" ? query.OrderByDescending(t => t.Harga) : query.OrderBy(t => t.Harga),
                    "tanggalevent" => orderState.ToLower() == "desc" ? query.OrderByDescending(t => t.TanggalEvent) : query.OrderBy(t => t.TanggalEvent),
                    _ => query.OrderBy(t => t.KodeTiket)
                };

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching available tickets: {ex.Message}");
                return new List<Tiket>();
            }
        }
    }
}