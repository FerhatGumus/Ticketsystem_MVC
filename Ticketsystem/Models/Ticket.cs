using Ticketsystem.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ticketsystem.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public int CustomerId { get; set; }
        public string? Subject { get; set; }
        public string? Priority { get; set; }
        public string? OrderNumber { get; set; }
        public string? ProductNumber { get; set; }
        public string? Status { get; set; }
        public string? Category { get; set; }
        public DateTime TicketDate { get; set; }
        public List<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }


    public class TicketComment
    {
        public int Id { get; set; }
        public string? Comment { get; set; }
        public DateTime DateAdded { get; set; }
        [ForeignKey("TicketId")]
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }
        public string? Name { get; set; }
        public TicketAttachment? Attachment { get; set; }
    }


    public class TicketAttachment
    {
        [Key]
        public int AttachmentId { get; set; }

        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }

        [Required]
        [StringLength(255)]
        public string FilePath { get; set; }
    }

}