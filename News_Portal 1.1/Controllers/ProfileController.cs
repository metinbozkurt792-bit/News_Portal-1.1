using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using News_Portal_1._1.DTOs;
using News_Portal_1._1.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace News_Portal_1._1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _env; 

        public ProfileController(UserManager<AppUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var currentUserName = User.Identity?.Name;
            if (currentUserName == null) return Unauthorized();

            var user = await _userManager.FindByNameAsync(currentUserName);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            return Ok(new
            {
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl 
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            var currentUserName = User.Identity?.Name;
            if (currentUserName == null) return Unauthorized();

            var user = await _userManager.FindByNameAsync(currentUserName);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Ok(new { Message = "Profil bilgileri başarıyla güncellendi!" });

            return BadRequest(new { Message = "Profil güncellenirken bir hata oluştu." });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var currentUserName = User.Identity?.Name;
            if (currentUserName == null) return Unauthorized();

            var user = await _userManager.FindByNameAsync(currentUserName);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (result.Succeeded)
                return Ok(new { Message = "Şifreniz başarıyla değiştirildi!" });

            return BadRequest(new { Message = "Şifre değiştirme başarısız. Mevcut şifrenizi yanlış girmiş olabilirsiniz." });
        }

        [HttpPost("upload-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var currentUserName = User.Identity?.Name;
            if (currentUserName == null) return Unauthorized();

            var user = await _userManager.FindByNameAsync(currentUserName);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "Lütfen geçerli bir dosya seçin." });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "profiles");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePictureUrl = "/images/profiles/" + fileName;
            await _userManager.UpdateAsync(user);

            return Ok(new { Message = "Profil fotoğrafınız başarıyla güncellendi!", NewImageUrl = user.ProfilePictureUrl });
        }
    }
}