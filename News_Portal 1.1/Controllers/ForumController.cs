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
    public class ForumController : ControllerBase
    {
        private readonly AuthRepository<ForumTopic> _topicRepo;
        private readonly AuthRepository<ForumReply> _replyRepo;

        public ForumController(AuthRepository<ForumTopic> topicRepo, AuthRepository<ForumReply> replyRepo)
        {
            _topicRepo = topicRepo;
            _replyRepo = replyRepo;
        }

        [HttpGet("topics")]
        public async Task<IActionResult> GetAllTopics()
        {
            var topics = await _topicRepo.GetAllAsync();
            var sortedTopics = topics.OrderByDescending(t => t.Id).ToList();
            return Ok(sortedTopics);
        }

        [HttpGet("topics/{id}")]
        public async Task<IActionResult> GetTopicById(int id)
        {
            var topic = await _topicRepo.GetByIdAsync(id);
            if (topic == null) return NotFound(new { Message = "Konu bulunamadı veya silinmiş!" });

            topic.ViewCount += 1; 
            await _topicRepo.UpdateAsync(topic);

            return Ok(topic);
        }

        [HttpPost("topics")]
        public async Task<IActionResult> CreateTopic([FromBody] ForumTopicCreateDto dto)
        {
            var currentUserName = User.Identity?.Name ?? "Anonim Kullanıcı";

            var newTopic = new ForumTopic
            {
                Title = dto.Title,
                Content = dto.Content,
                UserName = currentUserName
            };

            await _topicRepo.AddAsync(newTopic);
            return Ok(new { Message = "Yeni konu başarıyla açıldı!" });
        }

        [HttpGet("topics/{topicId}/replies")]
        public async Task<IActionResult> GetRepliesByTopicId(int topicId)
        {
            var allReplies = await _replyRepo.GetAllAsync();

            var topicReplies = allReplies.Where(r => r.TopicId == topicId)
                                         .OrderBy(r => r.Id)
                                         .ToList();

            return Ok(topicReplies);
        }

        [HttpPost("replies")]
        public async Task<IActionResult> CreateReply([FromBody] ForumReplyCreateDto dto)
        {
            var currentUserName = User.Identity?.Name ?? "Anonim Kullanıcı";

            var newReply = new ForumReply
            {
                TopicId = dto.TopicId,
                Content = dto.Content,
                UserName = currentUserName,
                ParentReplyId = dto.ParentReplyId
            };

            await _replyRepo.AddAsync(newReply);
            return Ok(new { Message = "Cevabınız başarıyla eklendi!" });
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPut("topics/{id}")]
        public async Task<IActionResult> UpdateTopic(int id, [FromBody] ForumTopicCreateDto dto)
        {
            var topic = await _topicRepo.GetByIdAsync(id);
            if (topic == null) return NotFound(new { Message = "Konu bulunamadı veya çoktan silinmiş!" });

            var currentUserName = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && topic.UserName != currentUserName)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden, new { Message = "Uyarı: Sadece kendi açtığınız konuları düzenleyebilirsiniz!" });
            }

            topic.Title = dto.Title;
            topic.Content = dto.Content;
            await _topicRepo.UpdateAsync(topic);

            return Ok(new { Message = "Konu başarıyla güncellendi!" });
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpDelete("topics/{id}")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            var topic = await _topicRepo.GetByIdAsync(id);
            if (topic == null) return NotFound(new { Message = "Konu bulunamadı!" });

            var currentUserName = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && topic.UserName != currentUserName)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden, new { Message = "Karargâh Uyarısı: Sadece kendi açtığınız konuları silebilirsiniz!" });
            }

            await _topicRepo.DeleteAsync(topic);
            return Ok(new { Message = "Konu sistemden tamamen silindi!" });
        }


        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPut("replies/{id}")]
        public async Task<IActionResult> UpdateReply(int id, [FromBody] ForumReplyCreateDto dto)
        {
            var reply = await _replyRepo.GetByIdAsync(id);
            if (reply == null) return NotFound(new { Message = "Cevap bulunamadı!" });

            if (reply.UserName != User.Identity?.Name && !User.IsInRole("Admin"))
                return StatusCode(403, new { Message = "Sadece kendi cevabınızı düzenleyebilirsiniz!" });

            reply.Content = dto.Content;
            await _replyRepo.UpdateAsync(reply);
            return Ok(new { Message = "Cevap başarıyla güncellendi!" });
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpDelete("replies/{id}")]
        public async Task<IActionResult> DeleteReply(int id)
        {
            var reply = await _replyRepo.GetByIdAsync(id);
            if (reply == null) return NotFound(new { Message = "Cevap bulunamadı!" });

            if (reply.UserName != User.Identity?.Name && !User.IsInRole("Admin"))
                return StatusCode(403, new { Message = "Sadece kendi cevabınızı silebilirsiniz!" });

            await _replyRepo.DeleteAsync(reply);
            return Ok(new { Message = "Cevap silindi!" });
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPost("replies/{id}/{actionType}")]
        public async Task<IActionResult> VoteReply(int id, string actionType)
        {
            var reply = await _replyRepo.GetByIdAsync(id);
            if (reply == null) return NotFound(new { Message = "Cevap bulunamadı!" });

            var currentUser = User.Identity?.Name ?? "Anonim";
            reply.LikedUsers ??= "";
            reply.DislikedUsers ??= "";

            bool isLiked = reply.LikedUsers.Contains(currentUser + ",");
            bool isDisliked = reply.DislikedUsers.Contains(currentUser + ",");

            string returnMessage = "";

            if (actionType == "like")
            {
                if (isLiked)
                {
                    reply.LikedUsers = reply.LikedUsers.Replace(currentUser + ",", "");
                    reply.LikeCount--;
                    returnMessage = "Beğeni geri alındı!";
                }
                else
                {
                    reply.LikedUsers += currentUser + ",";
                    reply.LikeCount++;
                    returnMessage = "Cevap beğenildi!";
                    if (isDisliked) { reply.DislikedUsers = reply.DislikedUsers.Replace(currentUser + ",", ""); reply.DislikeCount--; }
                }
            }
            else if (actionType == "dislike")
            {
                if (isDisliked)
                {
                    reply.DislikedUsers = reply.DislikedUsers.Replace(currentUser + ",", "");
                    reply.DislikeCount--;
                    returnMessage = "Beğenmeme durumu geri alındı!";
                }
                else
                {
                    reply.DislikedUsers += currentUser + ",";
                    reply.DislikeCount++;
                    returnMessage = "Cevap beğenilmedi!";
                    if (isLiked) { reply.LikedUsers = reply.LikedUsers.Replace(currentUser + ",", ""); reply.LikeCount--; }
                }
            }

            await _replyRepo.UpdateAsync(reply);
            return Ok(new { Message = returnMessage });
        }
    }
}