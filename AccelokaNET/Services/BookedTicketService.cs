using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccelokaDb.Entities.Context;
using AccelokaDb.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Acceloka.API.Services
{
    public class BookedTicketService
    {
        private readonly AccelokaDbContext _context;
        private readonly ILogger<BookedTicketService> _logger; 

        public BookedTicketService(AccelokaDbContext context, ILogger<BookedTicketService> logger)
        {
            _context = context;
            _logger = logger; 
        }

        public async Task<object> BookTicketsAsync(List<(string kodeTiket, int quantity)> bookingRequests)
        {
            var response = new List<object>();
            var ticketsByCategory = new Dictionary<string, List<(string ticketCode, string ticketName, decimal price)>>();
            decimal totalPriceSummary = 0;
            var currentDate = DateTime.UtcNow.ToLocalTime();

            foreach (var request in bookingRequests)
            {
                var ticket = await _context.Tikets.FirstOrDefaultAsync(t => t.KodeTiket == request.kodeTiket);
                if (ticket == null)
                {
                    _logger.LogWarning("Kode tiket tidak terdaftar: {KodeTiket}", request.kodeTiket);
                    return new { error = "Kode tiket tidak terdaftar" };
                }
                if (ticket.SisaQuota <= 0)
                {
                    _logger.LogWarning("Kode tiket quotanya habis: {KodeTiket}", request.kodeTiket); 
                    return new { error = "Kode tiket quotanya habis" };
                }
                if (ticket.SisaQuota < request.quantity)
                {
                    _logger.LogWarning("Quantity tiket ({Quantity}) melebihi sisa quota untuk tiket: {KodeTiket}", request.quantity, request.kodeTiket); 
                    return new { error = "Quantity tiket yang dibooking melebihi sisa quota" };
                }
                if (ticket.TanggalEvent.ToUniversalTime() <= currentDate)
                {
                    _logger.LogWarning("Tanggal event untuk tiket {KodeTiket} tidak valid, sudah lewat", request.kodeTiket);
                    return new { error = "Kode tiket yang dibooking tanggal eventnya tidak boleh <= tanggal booking tiket" };
                }

                var bookedTicket = new BookedTicket
                {
                    KodeTiket = request.kodeTiket,
                    Quantity = request.quantity,
                    BookedDate = currentDate
                };

                _context.BookedTickets.Add(bookedTicket);
                ticket.SisaQuota -= request.quantity;

                _logger.LogInformation("Tiket berhasil dibooking, Kode: {KodeTiket}, Quantity: {Quantity}", request.kodeTiket, request.quantity); 

                await _context.SaveChangesAsync();

                if (!ticketsByCategory.ContainsKey(ticket.NamaKategori))
                {
                    ticketsByCategory[ticket.NamaKategori] = new List<(string, string, decimal)>();
                }
                ticketsByCategory[ticket.NamaKategori].Add((ticket.KodeTiket, ticket.NamaTiket, ticket.Harga));
            }

            foreach (var category in ticketsByCategory)
            {
                decimal categoryPrice = category.Value.Sum(t => t.price);
                totalPriceSummary += categoryPrice;
                response.Add(new
                {
                    categoryName = category.Key,
                    summaryPrice = categoryPrice,
                    tickets = category.Value.Select(t => new { ticketCode = t.ticketCode, ticketName = t.ticketName, price = t.price })
                });
            }

            return new { priceSummary = totalPriceSummary, ticketsPerCategories = response };
        }

        public async Task<object> GetBookedTicketDetailsAsync(int bookedTicketId)
        {
            var bookedTickets = await _context.BookedTickets
                .Where(bt => bt.Id == bookedTicketId)
                .Join(_context.Tikets,
                    bt => bt.KodeTiket,
                    t => t.KodeTiket,
                    (bt, t) => new { bt, t })
                .ToListAsync();

            if (!bookedTickets.Any())
            {
                _logger.LogWarning("Tidak ada tiket yang ditemukan untuk BookedTicketId: {BookedTicketId}", bookedTicketId); 
                return null;
            }

            var groupedTickets = bookedTickets
                .GroupBy(bt => bt.t.NamaKategori)
                .Select(g => new
                {
                    categoryName = g.Key,
                    qtyPerCategory = g.Sum(bt => bt.bt.Quantity),
                    tickets = g.Select(bt => new
                    {
                        ticketCode = bt.t.KodeTiket,
                        ticketName = bt.t.NamaTiket,
                        eventDate = bt.t.TanggalEvent,
                        quantity = bt.bt.Quantity
                    }).ToList()
                }).ToList();

            return groupedTickets;
        }

        public async Task<object> RevokeTicketAsync(int bookedTicketId, string kodeTiket, int qty)
        {
            var bookedTicket = await _context.BookedTickets
                .FirstOrDefaultAsync(bt => bt.Id == bookedTicketId && bt.KodeTiket == kodeTiket);

            if (bookedTicket == null)
            {
                _logger.LogWarning("BookedTicketId atau KodeTiket tidak terdaftar: {BookedTicketId}, {KodeTiket}", bookedTicketId, kodeTiket); 
            }

            if (qty > bookedTicket.Quantity)
            {
                _logger.LogWarning("Qty yang ingin direvoke ({Qty}) melebihi jumlah yang dibooking sebelumnya ({BookedQuantity})", qty, bookedTicket.Quantity); 
                return new { error = "Qty yang ingin direvoke melebihi jumlah yang dibooking sebelumnya" };
            }

            var tiket = await _context.Tikets.FirstOrDefaultAsync(t => t.KodeTiket == kodeTiket);
            if (tiket == null)
            {
                _logger.LogWarning("Kode tiket tidak terdaftar: {KodeTiket}", kodeTiket);
                return new { error = "Kode tiket tidak terdaftar" };
            }

            tiket.SisaQuota += qty;
            bookedTicket.Quantity -= qty;

            if (bookedTicket.Quantity == 0)
            {
                _context.BookedTickets.Remove(bookedTicket);
            }

            await _context.SaveChangesAsync();

            bool isAllTicketsRevoked = !await _context.BookedTickets.AnyAsync(bt => bt.Id == bookedTicketId);
            if (isAllTicketsRevoked)
            {
                var bookedTicketsToRemove = await _context.BookedTickets.Where(bt => bt.Id == bookedTicketId).ToListAsync();
                _context.BookedTickets.RemoveRange(bookedTicketsToRemove);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Tiket berhasil dibatalkan, Kode: {KodeTiket}, Remaining Quantity: {RemainingQuantity}", tiket.KodeTiket, bookedTicket.Quantity); // Logging

            return new
            {
                ticketCode = tiket.KodeTiket,
                ticketName = tiket.NamaTiket,
                categoryName = tiket.NamaKategori,
                remainingQuantity = bookedTicket.Quantity
            };
        }

        public async Task<object> EditBookedTicketsAsync(int bookedTicketId, List<(string kodeTiket, int quantity)> updatedBookings)
        {
            var bookedTickets = await _context.BookedTickets
                .Where(bt => bt.Id == bookedTicketId)
                .ToListAsync();

            if (!bookedTickets.Any())
            {
                _logger.LogWarning("BookedTicketId tidak terdaftar: {BookedTicketId}", bookedTicketId);
                return new { error = "BookedTicketId tidak terdaftar" };
            }

            var responseList = new List<object>();

            foreach (var updatedBooking in updatedBookings)
            {
                var bookedTicket = bookedTickets.FirstOrDefault(bt => bt.KodeTiket == updatedBooking.kodeTiket);
                if (bookedTicket == null)
                {
                    _logger.LogWarning("Kode tiket tidak terdaftar dalam booking ini: {KodeTiket}", updatedBooking.kodeTiket); 
                    return new { error = "Kode tiket tidak terdaftar dalam booking ini" };
                }

                var ticket = await _context.Tikets.FirstOrDefaultAsync(t => t.KodeTiket == updatedBooking.kodeTiket);
                if (ticket == null)
                {
                    _logger.LogWarning("Kode tiket tidak terdaftar: {KodeTiket}", updatedBooking.kodeTiket); 
                    return new { error = "Kode tiket tidak terdaftar" };
                }

                if (updatedBooking.quantity > (ticket.SisaQuota + bookedTicket.Quantity))
                {
                    _logger.LogWarning("Quantity melebihi sisa quota tiket: {QuantityRequested}, Sisa: {SisaQuota}", updatedBooking.quantity, ticket.SisaQuota + bookedTicket.Quantity); // Logging
                    return new { error = "Quantity melebihi sisa quota tiket" };
                }

                if (updatedBooking.quantity < 1)
                {
                    _logger.LogWarning("Quantity minimal harus 1");
                    return new { error = "Quantity minimal harus 1" };
                }

                ticket.SisaQuota += bookedTicket.Quantity;
                ticket.SisaQuota -= updatedBooking.quantity;

                bookedTicket.Quantity = updatedBooking.quantity;

                responseList.Add(new
                {
                    ticketCode = ticket.KodeTiket,
                    ticketName = ticket.NamaTiket,
                    categoryName = ticket.NamaKategori,
                    remainingQuantity = bookedTicket.Quantity
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Jumlah tiket yang dibooking berhasil diperbarui untuk BookedTicketId: {BookedTicketId}", bookedTicketId); // Logging
            return responseList;
        }
    }
}