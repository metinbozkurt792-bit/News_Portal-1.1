using System;

namespace News_Portal_1._1.Models
{
    public class ForumReply
    {
        public int Id { get; set; }
        public int TopicId { get; set; } 
        public string Content { get; set; } 
        public string UserName { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public string? LikedUsers { get; set; }
        public string? DislikedUsers { get; set; }
        public int? ParentReplyId { get; set; }
    }
}