using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using News_Portal_1._1.DTOs;
using News_Portal_1._1.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace News_Portal_1._1.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    IsBanned = u.LockoutEnd != null
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpDelete("delete-user/{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            if (username.ToLower() == "admin")
            {
                return BadRequest(new { Message = "Sistem yöneticisi hesabı silinemez!" });
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound(new { Message = "Sistemde böyle bir kullanıcı yok!" });

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
                return Ok(new { Message = $"'{username}' adlı kullanıcı sistemden kalıcı olarak silindi." });

            return BadRequest(new { Message = "Kullanıcı silinirken bir hata oluştu." });
        }

        [HttpPost("ban-user/{username}")]
        public async Task<IActionResult> BanUser(string username)
        {
            if (username.ToLower() == "admin")
            {
                return BadRequest(new { Message = "Sistem yöneticisi banlanamaz!" });
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.UtcNow.AddYears(100));

            return Ok(new { Message = $"'{username}' adlı kullanıcı sistemden banlandı!" });
        }

        [HttpPost("unban-user/{username}")]
        public async Task<IActionResult> UnbanUser(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            await _userManager.SetLockoutEndDateAsync(user, null);

            return Ok(new { Message = $"'{username}' adlı kullanıcının banı başarıyla kaldırıldı." });
        }

        [HttpPost("upload-profile-picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Dosya seçilmedi.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            user.ProfilePicture = "/uploads/" + fileName;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { message = "Fotoğraf başarıyla yüklendi", path = user.ProfilePicture });
            }

            return BadRequest("Güncelleme hatası.");
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return Unauthorized("Kullanıcı bulunamadı");

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return NotFound("Kullanıcı yok");

            return Ok(new
            {
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                profilePicture = user.ProfilePicture
            });
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return Unauthorized(new { message = "Kimlik doğrulanamadı. Lütfen tekrar giriş yapın." });

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return NotFound(new { message = "Kullanıcı veritabanında bulunamadı." });

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    return BadRequest(new { message = "Şifrenizi değiştirmek için mevcut şifrenizi girmek zorundasınız!" });
                }

                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (!passwordChangeResult.Succeeded)
                {
                    var errorMsg = passwordChangeResult.Errors.FirstOrDefault()?.Description ?? "Şifre değiştirme işlemi başarısız oldu.";
                    return BadRequest(new { message = errorMsg });
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                return Ok(new { message = "Profiliniz başarıyla güncellendi!" });
            }

            return BadRequest(new { message = "Profil güncellenirken beklenmeyen bir hata oluştu." });
        }

        [HttpGet("dashboard-stats")]
        public IActionResult GetDashboardStats()
        {
            try
            {
                var newsCount = _context.News.Count();
                var categoryCount = _context.Categories.Count();
                var userCount = _userManager.Users.Count();

                return Ok(new
                {
                    newsCount = newsCount,
                    userCount = userCount,
                    categoryCount = categoryCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "İstatistikler çekilirken bir hata oluştu: " + ex.Message });
            }
        }
    }
}