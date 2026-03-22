using System;

namespace News_Portal_1._1.Models
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; } 
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int LikeCount { get; set; } = 0;
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public string? AuthorName { get; set; }
    }
}