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
            string imageUrl = ""; 

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);

                var uploadsFolder = Path.Combine(_env.WebRootPath, "images");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }
                imageUrl = "/images/" + fileName;
            }

            var newNews = new News
            {
                Title = dto.Title,
                Content = dto.Content,
                ImageUrl = imageUrl, 
                CategoryId = dto.CategoryId,
                AuthorName = authorName
            };

            await _repository.AddAsync(newNews);
            return Ok(new { Message = "Haber başarıyla eklendi!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNews(int id, [FromForm] NewsCreateDto dto)
        {
            var newsItem = await _repository.GetByIdAsync(id);
            if (newsItem == null)
                return NotFound(new { Message = "Güncellenecek haber bulunamadı!" });

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                newsItem.ImageUrl = "/images/" + fileName;
            }

            newsItem.Title = dto.Title;
            newsItem.Content = dto.Content;
            newsItem.CategoryId = dto.CategoryId;

            await _repository.UpdateAsync(newsItem);
            return Ok(new { Message = "Haber başarıyla güncellendi!" });
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

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeNews(int id)
        {
            var newsItem = await _repository.GetByIdAsync(id);
            if (newsItem == null)
                return NotFound(new { Message = "Beğenilecek haber bulunamadı!" });

            newsItem.LikeCount += 1;
            await _repository.UpdateAsync(newsItem);

            return Ok(new { Message = "Haber beğenildi!", CurrentLikes = newsItem.LikeCount });
        }
    }
}