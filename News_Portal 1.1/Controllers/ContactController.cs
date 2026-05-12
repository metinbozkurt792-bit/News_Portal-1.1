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
    public class ContactController : ControllerBase
    {
        private readonly AuthRepository<ContactMessage> _repository;

        public ContactController(AuthRepository<ContactMessage> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessageCreateDto dto)
        {
            var newMessage = new ContactMessage
            {
                SenderName = dto.SenderName,
                Email = dto.Email,
                Subject = dto.Subject,
                Message = dto.Message
            };

            await _repository.AddAsync(newMessage);
            return Ok(new { Message = "Mesajınız admine ulaştı! En kısa sürede dönüş yapılacaktır." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _repository.GetAllAsync();

            if (messages == null || !messages.Any())
            {
                return Ok(new List<ContactMessage>());
            }
            return Ok(messages.OrderByDescending(m => m.Id).ToList());
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var msg = await _repository.GetByIdAsync(id);
            if (msg == null) return NotFound(new { Message = "Mesaj bulunamadı." });

            msg.IsRead = true;
            await _repository.UpdateAsync(msg);
            return Ok(new { Message = "Mesaj okundu olarak işaretlendi." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var msg = await _repository.GetByIdAsync(id);
            if (msg == null) return NotFound(new { Message = "Mesaj bulunamadı." });

            await _repository.DeleteAsync(msg);
            return Ok(new { Message = "Mesaj silindi." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/reply")]
        public async Task<IActionResult> ReplyMessage(int id, [FromBody] ContactMessageCreateDto dto)
        {
            var parentMsg = await _repository.GetByIdAsync(id);
            if (parentMsg == null) return NotFound(new { Message = "Yanıtlanacak mesaj bulunamadı!" });

            var replyMsg = new ContactMessage
            {
                SenderName = "Admin", 
                Email = "admin@portal.com",
                Subject = "RE: " + parentMsg.Subject, 
                Message = dto.Message,
                ReceiverName = parentMsg.SenderName, 
                IsFromAdmin = true,
                ParentMessageId = parentMsg.Id
            };

            parentMsg.IsRead = true;
            await _repository.UpdateAsync(parentMsg);

            await _repository.AddAsync(replyMsg);
            return Ok(new { Message = "Yanıtınız kullanıcıya başarıyla iletildi!" });
        }

        [Authorize]
        [HttpGet("my-messages")]
        public async Task<IActionResult> GetMyMessages()
        {
            var currentUserName = User.Identity?.Name?.ToLower().Trim();

            if (string.IsNullOrEmpty(currentUserName)) return Unauthorized("Kimlik doğrulanamadı.");

            var allMessages = await _repository.GetAllAsync();

            var myMessages = allMessages
                .Where(m =>
                    (m.SenderName != null && m.SenderName.ToLower() == currentUserName) ||
                    (m.ReceiverName != null && m.ReceiverName.ToLower() == currentUserName))
                .OrderByDescending(m => m.Id)
                .ToList();

            return Ok(myMessages);
        }
    }
}