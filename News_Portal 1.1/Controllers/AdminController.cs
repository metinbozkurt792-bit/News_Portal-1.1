using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using News_Portal_1._1.Models; 
using System.Threading.Tasks;
using System.Linq;

namespace News_Portal_1._1.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
    }
}