namespace News_Portal_1._1.DTOs
{
    public class NewsCreateDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
        public int CategoryId { get; set; }
    }
}