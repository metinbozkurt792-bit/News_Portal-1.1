namespace News_Portal_1._1.DTOs
{
    public class ForumReplyCreateDto
    {
        public int TopicId { get; set; }
        public string Content { get; set; }
        public int? ParentReplyId { get; set; }
    }
}