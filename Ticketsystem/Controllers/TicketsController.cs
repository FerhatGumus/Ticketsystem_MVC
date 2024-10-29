using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Ticketsystem.Data;
using Ticketsystem.Models;
using System.Net.Sockets;

namespace Ticketsystem.Controllers
{
    public class TicketsController : Controller
    {

        private readonly TicketSystemDbContext _ticketcontext;

        public TicketsController(TicketSystemDbContext ticketcontext)
        {
            _ticketcontext = ticketcontext;
        }

        [HttpPost]
        public IActionResult CreateTicket(Ticket ticket, string newCommentText, IFormFile uploadedFile)
        {

            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                // Speichert die Datei im folgendem Pfad
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uploadedFile.FileName);

                // Datei speichern
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadedFile.CopyTo(stream);
                }

                // Optional
                ViewBag.Message = "Datei erfolgreich hochgeladen!";
            }

            ticket.Status = "Offen";
            if (!string.IsNullOrWhiteSpace(newCommentText))
            {
                var newComment = new TicketComment
                {
                    Name = "User",
                    Comment = newCommentText,
                    DateAdded = DateTime.Now,
                };

                ticket.Comments.Add(newComment);
            }
            ticket.TicketDate = DateTime.Now;

            _ticketcontext.Tickets.Add(ticket);
            _ticketcontext.SaveChanges();

            return RedirectToAction("Dashboard");
        }


        public IActionResult CreateTicket(string Kategorie, string Thema, string Prioritaet)
        {
            ViewBag.Kategorie = Kategorie;
            ViewBag.Thema = Thema;
            ViewBag.Prioritaet = Prioritaet;

            return View();
        }

        public IActionResult Edit(int id)
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
        public IActionResult Edit(int id, string newCommentText, string newState, IFormFile uploadedFile)
        {
            var ticket = _ticketcontext.Tickets
                .Include(t => t.Comments)  // Kommentare mitladen
                .ThenInclude(c => c.Attachment) // Anhänge mitladen
                .FirstOrDefault(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // Prüft ob eine Datei hochgeladen wurde
            string? filePath = null;
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                // Dateiname und Pfad festlegen
                filePath = Path.Combine("wwwroot/uploads/", uploadedFile.FileName);

                // Datei speichern
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadedFile.CopyTo(stream);
                }
                filePath = "/uploads/" + uploadedFile.FileName; // Web-Pfad speichern
            }

            // Neuen Kommentar hinzufügen, wenn vorhanden
            if (!string.IsNullOrWhiteSpace(newCommentText))
            {
                var newComment = new TicketComment
                {
                    Name = "User",
                    Comment = newCommentText,
                    DateAdded = DateTime.Now,
                    TicketId = ticket.TicketId
                };

                // Falls ein Bild hochgeladen wurde, einen Anhang hinzufügen
                if (!string.IsNullOrEmpty(filePath))
                {
                    newComment.Attachment = new TicketAttachment
                    {
                        FilePath = filePath
                    };
                }

                ticket.Comments.Add(newComment);
                _ticketcontext.TicketComments.Add(newComment); // Speichert den Kommentar mit dem Anhang
            }

            // Status aktualisieren
            ticket.Status = newState;

            _ticketcontext.SaveChanges();

            return RedirectToAction(nameof(Edit), new { id = ticket.TicketId });
        }




        public IActionResult Delete(int id) 
        {
            if (id > 0)
            {
                var ticket = _ticketcontext.Tickets.Find(id);

                _ticketcontext.Tickets.Remove(ticket);
                _ticketcontext.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        public IActionResult Dashboard()
        {
            var allTickets = _ticketcontext.Tickets.ToArray();

            // Filtern Sie die Anzahl der offenen Tickets (angenommen "Offen" ist der Wert für offene Tickets im Status)
            var offeneTickets = allTickets.Count(t => t.Status == "Offen");
            var inBearbeitungTickets = allTickets.Count(t => t.Status == "In Bearbeitung");
            var GeschlosseneTickets = allTickets.Count(t => t.Status == "Geschlossen");

            // Speichern Sie die Anzahl der offenen Tickets in ViewBag oder ViewData
            ViewBag.AnzahlOffeneTickets = offeneTickets;
            ViewBag.AnzahlInBearbeitungTickets = inBearbeitungTickets;
            ViewBag.AnzahlGeschlosseneTickets = GeschlosseneTickets;


            return View(allTickets);
        }
    }
}
