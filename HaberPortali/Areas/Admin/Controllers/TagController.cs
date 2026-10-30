using HaberPortali.Helpers;
using HaberPortali.Models;
using HaberPortali.Repositories;
using HaberPortali.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaberPortali.Areas.Admin.Controllers
{
    // Etiket sayfası tamamen AJAX ile çalışır: ekleme ve silme sayfa yenilenmeden yapılır.
    [Area("Admin")]
    [Authorize(Roles = Roles.AdminOrEditor)]
    public class TagController : Controller
    {
        private readonly IRepository<Tag> _tagRepository;

        public TagController(IRepository<Tag> tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _tagRepository.Query()
                .OrderBy(t => t.Name)
                .Select(t => new TagListItemViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    NewsCount = t.NewsTags.Count
                })
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? name)
        {
            name = name?.Trim();

            if (string.IsNullOrEmpty(name) || name.Length < 2 || name.Length > 50)
            {
                return Json(new { success = false, message = "Etiket adı 2 ile 50 karakter arasında olmalıdır." });
            }

            if (await _tagRepository.AnyAsync(t => t.Name == name))
            {
                return Json(new { success = false, message = "Bu etiket zaten var." });
            }

            var tag = new Tag { Name = name, Slug = SlugHelper.ToSlug(name) };
            await _tagRepository.AddAsync(tag);
            await _tagRepository.SaveAsync();

            return Json(new
            {
                success = true,
                message = $"\"{tag.Name}\" etiketi eklendi.",
                tag = new { id = tag.Id, name = tag.Name, slug = tag.Slug, newsCount = 0 }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return Json(new { success = false, message = "Etiket bulunamadı." });
            }

            // Ara tablodaki (NewsTags) kayıtlar cascade ile otomatik silinir.
            _tagRepository.Delete(tag);
            await _tagRepository.SaveAsync();

            return Json(new { success = true, message = $"\"{tag.Name}\" etiketi silindi." });
        }
    }
}
