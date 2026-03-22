using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using News_Portal_1._1.DTOs;
using News_Portal_1._1.Models;
using News_Portal_1._1.Repositories;
using System.Threading.Tasks;

namespace News_Portal_1._1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AuthRepository<Category> _repository;

        public CategoryController(AuthRepository<Category> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _repository.GetAllAsync();
            if (categories == null || !categories.Any())
            {
                return NotFound(new { Message = "Sistemde henüz hiçbir kategori bulunmamaktadır." });
            }
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { Message = "Kategori bulunamadı!" });

            return Ok(category);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CategoryCreateDto dto)
        {
            var newCategory = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _repository.AddAsync(newCategory);
            return Ok(new { Message = "Kategori başarıyla eklendi!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _repository.GetByIdAsync(id);

            if (category == null)
                return NotFound(new { Message = "Silinecek kategori bulunamadı!" });


            await _repository.DeleteAsync(category);
            return Ok(new { Message = "Kategori başarıyla silindi." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryCreateDto dto)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { Message = "Güncellenecek kategori bulunamadı!" });

            category.Name = dto.Name;
            category.Description = dto.Description;

            await _repository.UpdateAsync(category); 
            return Ok(new { Message = "Kategori başarıyla güncellendi!" });
        }
    }
}