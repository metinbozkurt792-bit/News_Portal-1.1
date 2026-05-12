using System;

namespace News_Portal_1._1.Models
{
    public class ForumTopic
    {
        public int Id { get; set; }
        public string Title { get; set; } 
        public string Content { get; set; } 
        public string UserName { get; set; } 
        public int ViewCount { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
    }
}