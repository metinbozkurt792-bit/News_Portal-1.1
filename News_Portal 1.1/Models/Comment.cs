using System;

namespace News_Portal_1._1.Models
{
    public class Comment
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string UserName { get; set; }
        public int NewsId { get; set; }
        public virtual News News { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public string? LikedUsers { get; set; }   
        public string? DislikedUsers { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}