using HaberPortali.Models;
using HaberPortali.Repositories;
using HaberPortali.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaberPortali.Controllers
{
    public class HomeController : Controller
    {
        private readonly INewsRepository _newsRepository;
        private readonly IRepository<Category> _categoryRepository;

        public HomeController(INewsRepository newsRepository, IRepository<Category> categoryRepository)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var headlines = await _newsRepository.Published()
                .Where(n => n.IsHeadline)
                .OrderByDescending(n => n.PublishedAt)
                .Take(5)
                .ToListAsync();

            var headlineIds = headlines.Select(n => n.Id).ToList();

            var latest = await _newsRepository.Published()
                .Where(n => !headlineIds.Contains(n.Id))
                .OrderByDescending(n => n.PublishedAt)
                .Take(6)
                .ToListAsync();

            var mostRead = await _newsRepository.Published()
                .OrderByDescending(n => n.ViewCount)
                .Take(5)
                .ToListAsync();

            var categories = await _categoryRepository.Query()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var sections = new List<CategorySection>();
            foreach (var category in categories)
            {
                var items = await _newsRepository.Published()
                    .Where(n => n.CategoryId == category.Id)
                    .OrderByDescending(n => n.PublishedAt)
                    .Take(4)
                    .ToListAsync();

                if (items.Count > 0)
                {
                    sections.Add(new CategorySection { Category = category, Items = items });
                }
            }

            var model = new HomeViewModel
            {
                Headlines = headlines,
                Latest = latest,
                MostRead = mostRead,
                Sections = sections
            };

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        // 404 gibi durum kodları için özel sayfa (Program.cs içinde UseStatusCodePagesWithReExecute).
        public IActionResult Status(int code)
        {
            Response.StatusCode = code;
            return View(code);
        }

        [Route("/Home/Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
