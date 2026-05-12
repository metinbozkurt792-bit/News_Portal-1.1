using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc;
using News_Portal_1._1.DTOs;
using News_Portal_1._1.Models;
using News_Portal_1._1.Repositories;
using System;
using System.IO; 
using System.Threading.Tasks;
using System.Linq;

namespace News_Portal_1._1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly AuthRepository<News> _repository;
        private readonly IWebHostEnvironment _env; 
        public NewsController(AuthRepository<News> repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNews()
        {
            var newsList = await _repository.GetAllAsync();

            if (newsList == null || !newsList.Any())
            {
                return NotFound(new { Message = "Sistemde henüz hiçbir haber bulunmamaktadır." });
            }
            var sortedNews = newsList.OrderByDescending(n => n.Id).ToList();

            return Ok(sortedNews);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(int id)
        {
            var newsItem = await _repository.GetByIdAsync(id);
            if (newsItem == null)
                return NotFound(new { Message = "Haber bulunamadı!" });

            newsItem.ViewCount += 1;
            await _repository.UpdateAsync(newsItem);

            return Ok(newsItem);
        }
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetNewsByCategory(int categoryId)
        {
            var allNews = await _repository.GetAllAsync();

            var categoryNews = allNews.Where(n => n.CategoryId == categoryId)
                                      .OrderByDescending(n => n.Id) 
                                      .ToList();

            if (!categoryNews.Any())
                return NotFound(new { Message = "Bu kategoride henüz haber bulunmuyor." });

            return Ok(categoryNews);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddNews([FromForm] NewsCreateDto dto)
        {
            var authorName = User.Identity?.Name ?? "Sistem Yöneticisi";

            List<string> uploadedImageUrls = new List<string>();

            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                foreach (var file in dto.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        uploadedImageUrls.Add("/images/" + fileName);
                    }
                }
            }

            string finalImageUrls = string.Join(",", uploadedImageUrls);

            var newNews = new News
            {
                Title = dto.Title,
                Content = dto.Content,
                ImageUrl = finalImageUrls, 
                CategoryId = dto.CategoryId,
                AuthorName = authorName
            };

            await _repository.AddAsync(newNews);
            return Ok(new { Message = "Haber ve tüm görseller başarıyla eklendi!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNews(int id, [FromForm] NewsCreateDto dto)
        {
            var newsItem = await _repository.GetByIdAsync(id);
            if (newsItem == null) return NotFound(new { Message = "Haber bulunamadı!" });

            if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
            {
                List<string> newPhotos = new List<string>();
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                foreach (var file in dto.ImageFiles)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }
                    newPhotos.Add("/images/" + fileName);
                }

                string newPhotosString = string.Join(",", newPhotos);

                if (string.IsNullOrEmpty(newsItem.ImageUrl))
                {
                    newsItem.ImageUrl = newPhotosString;
                }
                else
                {
                    newsItem.ImageUrl += "," + newPhotosString;
                }
            }

            newsItem.Title = dto.Title;
            newsItem.Content = dto.Content;
            newsItem.CategoryId = dto.CategoryId;

            await _repository.UpdateAsync(newsItem);
            return Ok(new { Message = "Haber ve yeni görseller başarıyla güncellendi!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var newsItem = await _repository.GetByIdAsync(id);
            if (newsItem == null)
                return NotFound(new { Message = "Silinecek haber bulunamadı!" });


            await _repository.DeleteAsync(newsItem);
            return Ok(new { Message = "Haber başarıyla silindi." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/delete-image")]
        public async Task<IActionResult> DeleteSingleImage(int id, [FromQuery] string imageUrl)
        {
            var newsItem = await _repository.GetByIdAsync(id);
            if (newsItem == null) return NotFound(new { Message = "Haber bulunamadı!" });

            if (string.IsNullOrEmpty(newsItem.ImageUrl))
                return BadRequest(new { Message = "Bu haberin zaten görseli yok!" });

            var images = newsItem.ImageUrl.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (images.Contains(imageUrl))
            {
                images.Remove(imageUrl);
                newsItem.ImageUrl = images.Count > 0 ? string.Join(",", images) : "";
                var physicalPath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }

                await _repository.UpdateAsync(newsItem);
                return Ok(new { Message = "Görsel başarıyla kaldırıldı!" });
            }
            return BadRequest(new { Message = "Silinmek istenen görsel bu haberde bulunamadı!" });
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeNews(int id)
        {
            var newsItem = await _repository.GetByIdAsync(id);
            if (newsItem == null) return NotFound(new { Message = "Haber bulunamadı!" });

            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "BilinmeyenIP";
            if (!string.IsNullOrEmpty(newsItem.LikedIPs) && newsItem.LikedIPs.Contains(userIp))
            {
                return BadRequest(new { Message = "Sistem Uyarısı: Bu haberi zaten beğendiniz!" });
            }

            newsItem.LikeCount += 1;
            newsItem.LikedIPs += userIp + ",";

            await _repository.UpdateAsync(newsItem);

            return Ok(new { Message = "Haber beğenildi!", CurrentLikes = newsItem.LikeCount });
        }

        [HttpPost("{id}/unlike")]
        public async Task<IActionResult> UnlikeNews(int id)
        {
            var newsItem = await _repository.GetByIdAsync(id);
            if (newsItem == null) return NotFound(new { Message = "Haber bulunamadı!" });

            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "BilinmeyenIP";

            if (!string.IsNullOrEmpty(newsItem.LikedIPs) && newsItem.LikedIPs.Contains(userIp))
            {
                newsItem.LikeCount -= 1;
                newsItem.LikedIPs = newsItem.LikedIPs.Replace(userIp + ",", "");

                await _repository.UpdateAsync(newsItem);
            }
            return Ok(new { Message = "Beğeni geri alındı!", CurrentLikes = newsItem.LikeCount });
        }
    }
}