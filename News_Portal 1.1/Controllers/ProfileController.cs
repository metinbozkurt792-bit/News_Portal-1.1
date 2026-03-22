using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using News_Portal_1._1.DTOs;
using News_Portal_1._1.Models;
using System.Threading.Tasks;

namespace News_Portal_1._1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var currentUserName = User.Identity?.Name;
            if (currentUserName == null) return Unauthorized();

            var user = await _userManager.FindByNameAsync(currentUserName);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            return Ok(new
            {
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            var currentUserName = User.Identity?.Name;
            if (currentUserName == null) return Unauthorized(); 

            var user = await _userManager.FindByNameAsync(currentUserName);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Ok(new { Message = "Profil bilgileri başarıyla güncellendi!" });

            return BadRequest("Profil güncellenirken bir hata oluştu.");
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var currentUserName = User.Identity?.Name;
            if (currentUserName == null) return Unauthorized(); 

            var user = await _userManager.FindByNameAsync(currentUserName);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (result.Succeeded)
                return Ok(new { Message = "Şifreniz başarıyla değiştirildi!" });

            return BadRequest(new { Message = "Şifre değiştirme başarısız. Mevcut şifrenizi yanlış girmiş olabilirsiniz." });
        }
    }
}