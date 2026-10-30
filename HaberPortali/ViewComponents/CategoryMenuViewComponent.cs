using HaberPortali.Models;
using HaberPortali.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaberPortali.ViewComponents
{
    // Sitenin üst menüsündeki kategori bağlantıları (her sayfada kullanılır).
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoryMenuViewComponent(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(string view = "Default")
        {
            var categories = await _categoryRepository.Query()
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(view, categories);
        }
    }
}
