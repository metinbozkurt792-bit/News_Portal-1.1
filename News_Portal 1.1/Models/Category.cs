using System.Collections.Generic;

namespace News_Portal_1._1.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } 
        public virtual ICollection<News> NewsList { get; set; }
    }
}