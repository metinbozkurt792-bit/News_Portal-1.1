using System;

namespace News_Portal_1._1.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string Email { get; set; } 
        public string Subject { get; set; } 
        public string Message { get; set; } 
        public bool IsRead { get; set; } = false; 
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
        public string? ReceiverName { get; set; } 
        public bool IsFromAdmin { get; set; } = false; 
        public int? ParentMessageId { get; set; }
    }
}