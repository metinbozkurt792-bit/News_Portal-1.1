using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using News_Portal_1._1.DTOs;
using News_Portal_1._1.Models;
using News_Portal_1._1.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace News_Portal_1._1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly AuthRepository<Comment> _repository;

        public CommentController(AuthRepository<Comment> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllComments()
        {
            var comments = await _repository.GetAllAsync();

            if (comments == null || !comments.Any())
                return NotFound(new { Message = "Sistemde henüz hiçbir yorum bulunmamaktadır." });

            var sortedComments = comments.OrderByDescending(c => c.Id).ToList();

            return Ok(sortedComments);
        }

        [HttpGet("news/{newsId}")]
        public async Task<IActionResult> GetCommentsByNewsId(int newsId)
        {
            var allComments = await _repository.GetAllAsync();

            var newsComments = allComments.Where(c => c.NewsId == newsId)
                                          .OrderByDescending(c => c.Id)
                                          .ToList();

            if (!newsComments.Any())
                return NotFound(new { Message = "Bu habere henüz yorum yapılmamış." });

            return Ok(newsComments);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentCreateDto dto)
        {
            var currentUserName = User.Identity?.Name ?? "Anonim";

            var newComment = new Comment
            {
                NewsId = dto.NewsId,
                Text = dto.Text,
                UserName = currentUserName,
                CreatedAt = DateTime.Now
            };

            await _repository.AddAsync(newComment);
            return Ok(new { Message = "Yorumunuz başarıyla eklendi!" });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] CommentCreateDto dto)
        {
            var comment = await _repository.GetByIdAsync(id);
            if (comment == null)
                return NotFound(new { Message = "Güncellenecek yorum bulunamadı!" });

            var currentUserName = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && comment.UserName != currentUserName)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Sadece kendi yorumlarınızı güncelleyebilirsiniz!" });
            }
            comment.Text = dto.Text;

            await _repository.UpdateAsync(comment);
            return Ok(new { Message = "Yorum başarıyla güncellendi." });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _repository.GetByIdAsync(id);
            if (comment == null)
                return NotFound(new { Message = "Yorum bulunamadı!" });

            var currentUserName = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && comment.UserName != currentUserName)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Sadece kendi yorumlarınızı silebilirsiniz!" });
            }

            await _repository.DeleteAsync(comment);
            return Ok(new { Message = "Yorum başarıyla silindi." });
        }

        [Authorize]
        [HttpPost("{id}/{actionType}")]
        public async Task<IActionResult> VoteComment(int id, string actionType)
        {
            var comment = await _repository.GetByIdAsync(id);
            if (comment == null) return NotFound(new { Message = "Yorum bulunamadı!" });

            var currentUser = User.Identity?.Name ?? "Anonim";

            comment.LikedUsers ??= "";
            comment.DislikedUsers ??= "";

            bool isLiked = comment.LikedUsers.Contains(currentUser + ",");
            bool isDisliked = comment.DislikedUsers.Contains(currentUser + ",");

            if (actionType == "like")
            {
                if (isLiked)
                {
                    comment.LikedUsers = comment.LikedUsers.Replace(currentUser + ",", "");
                    comment.LikeCount--;
                    await _repository.UpdateAsync(comment);
                    return Ok(new { Message = "Beğeni geri alındı!", Action = "removed" });
                }
                comment.LikedUsers += currentUser + ",";
                comment.LikeCount++;

                if (isDisliked) 
                {
                    comment.DislikedUsers = comment.DislikedUsers.Replace(currentUser + ",", "");
                    comment.DislikeCount--;
                }
            }
            else if (actionType == "dislike")
            {
                if (isDisliked)
                {
                    comment.DislikedUsers = comment.DislikedUsers.Replace(currentUser + ",", "");
                    comment.DislikeCount--;
                    await _repository.UpdateAsync(comment);
                    return Ok(new { Message = "Beğenmeme oyu geri alındı!", Action = "removed" });
                }
                comment.DislikedUsers += currentUser + ",";
                comment.DislikeCount++;

                if (isLiked)
                {
                    comment.LikedUsers = comment.LikedUsers.Replace(currentUser + ",", "");
                    comment.LikeCount--;
                }
            }
            else
            {
                return BadRequest(new { Message = "Geçersiz işlem tipi!" });
            }

            await _repository.UpdateAsync(comment);
            return Ok(new { Message = "Oyunuz başarıyla kaydedildi!", Action = "voted" });
        }
    }
}