using HaberPortali.Helpers;
using HaberPortali.Models;
using HaberPortali.Repositories;
using HaberPortali.Services;
using HaberPortali.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HaberPortali.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.AdminOrEditor)]
    public class NewsController : Controller
    {
        private const int PageSize = 10;

        private readonly INewsRepository _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IImageService _imageService;

        public NewsController(INewsRepository newsRepository, IRepository<Category> categoryRepository, IImageService imageService)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _imageService = imageService;
        }

        // GET: /Admin/News?search=...&categoryId=...&page=2
        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            if (page < 1) page = 1;

            var (items, total) = await _newsRepository.GetPagedAsync(search, categoryId, page, PageSize, onlyPublished: false);

            var model = new NewsListViewModel
            {
                Items = items,
                Categories = (await _categoryRepository.GetAllAsync()).OrderBy(c => c.DisplayOrder).ToList(),
                Search = search,
                CategoryId = categoryId,
                Page = page,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new NewsFormViewModel
            {
                PublishedAt = DateTime.Now,
                IsActive = true
            };
            await FillCategoriesAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsFormViewModel model)
        {
            ValidateImage(model);

            if (!ModelState.IsValid)
            {
                await FillCategoriesAsync(model);
                return View(model);
            }

            var news = new News
            {
                Title = model.Title.Trim(),
                Slug = SlugHelper.ToSlug(model.Title),
                Summary = model.Summary.Trim(),
                Content = model.Content.Trim(),
                CategoryId = model.CategoryId,
                PublishedAt = model.PublishedAt,
                IsHeadline = model.IsHeadline,
                IsBreaking = model.IsBreaking,
                IsActive = model.IsActive,
                AuthorId = User.GetUserId()
            };

            if (model.ImageFile != null)
            {
                news.ImagePath = await _imageService.SaveAsync(model.ImageFile, "news");
            }

            await _newsRepository.AddAsync(news);
            await _newsRepository.SetTagsAsync(news, model.TagsText);
            await _newsRepository.SaveAsync();

            TempData["Success"] = "Haber başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _newsRepository.GetDetailAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            var model = new NewsFormViewModel
            {
                Id = news.Id,
                Title = news.Title,
                Summary = news.Summary,
                Content = news.Content,
                CategoryId = news.CategoryId,
                PublishedAt = news.PublishedAt,
                IsHeadline = news.IsHeadline,
                IsBreaking = news.IsBreaking,
                IsActive = news.IsActive,
                ExistingImagePath = news.ImagePath,
                TagsText = string.Join(", ", news.NewsTags.Select(nt => nt.Tag.Name))
            };

            await FillCategoriesAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NewsFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ValidateImage(model);

            var news = await _newsRepository.GetDetailAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.ExistingImagePath = news.ImagePath;
                await FillCategoriesAsync(model);
                return View(model);
            }

            news.Title = model.Title.Trim();
            news.Slug = SlugHelper.ToSlug(model.Title);
            news.Summary = model.Summary.Trim();
            news.Content = model.Content.Trim();
            news.CategoryId = model.CategoryId;
            news.PublishedAt = model.PublishedAt;
            news.IsHeadline = model.IsHeadline;
            news.IsBreaking = model.IsBreaking;
            news.IsActive = model.IsActive;
            news.UpdatedAt = DateTime.Now;

            if (model.ImageFile != null)
            {
                _imageService.Delete(news.ImagePath);
                news.ImagePath = await _imageService.SaveAsync(model.ImageFile, "news");
            }
            else if (model.RemoveImage)
            {
                _imageService.Delete(news.ImagePath);
                news.ImagePath = null;
            }

            await _newsRepository.SetTagsAsync(news, model.TagsText);
            await _newsRepository.SaveAsync();

            TempData["Success"] = "Haber güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Haber silme yalnızca yöneticiye açıktır.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            _imageService.Delete(news.ImagePath);
            _newsRepository.Delete(news);
            await _newsRepository.SaveAsync();

            TempData["Success"] = "Haber silindi.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX: liste sayfasındaki anahtarlarla yayın, manşet ve son dakika durumunu değiştirir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id, string field)
        {
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null)
            {
                return Json(new { success = false, message = "Haber bulunamadı." });
            }

            bool value;
            string label;
            switch (field)
            {
                case "active":
                    news.IsActive = !news.IsActive;
                    value = news.IsActive;
                    label = value ? "Haber yayına alındı." : "Haber yayından kaldırıldı.";
                    break;
                case "headline":
                    news.IsHeadline = !news.IsHeadline;
                    value = news.IsHeadline;
                    label = value ? "Haber manşete eklendi." : "Haber manşetten çıkarıldı.";
                    break;
                case "breaking":
                    news.IsBreaking = !news.IsBreaking;
                    value = news.IsBreaking;
                    label = value ? "Haber son dakika bandına eklendi." : "Haber son dakika bandından çıkarıldı.";
                    break;
                default:
                    return Json(new { success = false, message = "Geçersiz işlem." });
            }

            await _newsRepository.SaveAsync();
            return Json(new { success = true, value, message = label });
        }

        private void ValidateImage(NewsFormViewModel model)
        {
            if (model.ImageFile != null && !_imageService.IsValid(model.ImageFile, out var error))
            {
                ModelState.AddModelError(nameof(model.ImageFile), error);
            }
        }

        private async Task FillCategoriesAsync(NewsFormViewModel model)
        {
            var categories = await _categoryRepository.GetAllAsync();
            model.Categories = categories
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.IsActive ? c.Name : c.Name + " (pasif)"
                })
                .ToList();
        }
    }
}
