using System;

namespace News_Portal_1._1.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string UserName { get; set; }
        public int NewsId { get; set; }
        public virtual News News { get; set; }
    }
}