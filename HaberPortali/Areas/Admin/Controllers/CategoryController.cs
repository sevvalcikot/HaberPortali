using HaberPortali.Helpers;
using HaberPortali.Models;
using HaberPortali.Repositories;
using HaberPortali.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaberPortali.Areas.Admin.Controllers
{
    // Kategori yönetimi yalnızca yöneticiye açıktır; editörler giremez.
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _categoryRepository.Query()
                .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
                .Select(c => new CategoryListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    NewsCount = c.News.Count
                })
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var nextOrder = await _categoryRepository.Query().Select(c => (int?)c.DisplayOrder).MaxAsync() ?? 0;
            return View(new CategoryFormViewModel { DisplayOrder = nextOrder + 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            await ValidateUniqueAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                Slug = SlugHelper.ToSlug(model.Name),
                Description = model.Description?.Trim(),
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();

            TempData["Success"] = $"\"{category.Name}\" kategorisi eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(new CategoryFormViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            await ValidateUniqueAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = model.Name.Trim();
            category.Slug = SlugHelper.ToSlug(model.Name);
            category.Description = model.Description?.Trim();
            category.DisplayOrder = model.DisplayOrder;
            category.IsActive = model.IsActive;

            await _categoryRepository.SaveAsync();

            TempData["Success"] = "Kategori güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetFirstAsync(c => c.Id == id, c => c.News);
            if (category == null)
            {
                return NotFound();
            }

            if (category.News.Any())
            {
                TempData["Error"] = $"\"{category.Name}\" kategorisinde {category.News.Count} haber var. Önce haberleri başka kategoriye taşıyın ya da kategoriyi pasif yapın.";
                return RedirectToAction(nameof(Index));
            }

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveAsync();

            TempData["Success"] = "Kategori silindi.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX: kategoriyi aktif/pasif yapar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return Json(new { success = false, message = "Kategori bulunamadı." });
            }

            category.IsActive = !category.IsActive;
            await _categoryRepository.SaveAsync();

            return Json(new
            {
                success = true,
                value = category.IsActive,
                message = category.IsActive ? "Kategori aktif edildi." : "Kategori pasif yapıldı."
            });
        }

        private async Task ValidateUniqueAsync(CategoryFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return;
            }

            var slug = SlugHelper.ToSlug(model.Name);
            if (await _categoryRepository.AnyAsync(c => c.Slug == slug && c.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "Bu isimde bir kategori zaten var.");
            }
        }
    }
}
