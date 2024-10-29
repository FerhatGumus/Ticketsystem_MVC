using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketsystem.Data;
using Ticketsystem.Models;

namespace Ticketsystem.Controllers
{
    public class TicketsAdminController : Controller
    {

        private readonly TicketSystemDbContext _ticketcontext;

        public TicketsAdminController (TicketSystemDbContext ticketcontext)
        {
            _ticketcontext = ticketcontext;
        }

        [HttpGet]
        public IActionResult DashboardAdmin()
        {
            var allTickets = _ticketcontext.Tickets.ToArray();

            // Filtern Sie die Anzahl der offenen Tickets
            var offeneTickets = allTickets.Count(t => t.Status == "Offen");
            var inBearbeitungTickets = allTickets.Count(t => t.Status == "In Bearbeitung");
            var GeschlosseneTickets = allTickets.Count(t => t.Status == "Geschlossen");

            // Speichert Anzahl der offenen Tickets
            ViewBag.AnzahlOffeneTickets = offeneTickets;
            ViewBag.AnzahlInBearbeitungTickets = inBearbeitungTickets;
            ViewBag.AnzahlGeschlosseneTickets = GeschlosseneTickets;


            return View(allTickets);
        }

        [HttpGet]
        public IActionResult EditAdmin(int id)
        {
            var ticket = _ticketcontext.Tickets
                .Include(t => t.Comments)  // Kommentare mitladen
                .FirstOrDefault(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }



        [HttpPost]
        public IActionResult EditAdmin(int id, string newCommentText, string newState)
        {
            var ticket = _ticketcontext.Tickets.Include(t => t.Comments).FirstOrDefault(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // Neuen Kommentar hinzufügen
            if (!string.IsNullOrWhiteSpace(newCommentText))
            {
                var newComment = new TicketComment
                {
                    Name = "Admin",
                    Comment = newCommentText,
                    DateAdded = DateTime.Now,
                    TicketId = ticket.TicketId 
                };

                _ticketcontext.TicketComments.Add(newComment);
                _ticketcontext.SaveChanges();
            }

            ticket.Status = newState;
            _ticketcontext.SaveChanges();

            return View(ticket);
        }
    }
}
