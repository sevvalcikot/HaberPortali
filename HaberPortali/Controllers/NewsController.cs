using HaberPortali.Models;
using HaberPortali.Repositories;
using HaberPortali.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaberPortali.Controllers
{
    // Sitenin haber sayfaları: haber detayı, kategori, etiket ve arama.
    public class NewsController : Controller
    {
        private const int PageSize = 9;

        private readonly INewsRepository _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Tag> _tagRepository;
        private readonly IRepository<Comment> _commentRepository;

        public NewsController(
            INewsRepository newsRepository,
            IRepository<Category> categoryRepository,
            IRepository<Tag> tagRepository,
            IRepository<Comment> commentRepository)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _commentRepository = commentRepository;
        }

        // GET: /haber/5/haber-basligi
        public async Task<IActionResult> Detail(int id, string? slug)
        {
            var news = await _newsRepository.Published()
                .Include(n => n.Author)
                .Include(n => n.NewsTags).ThenInclude(nt => nt.Tag)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null)
            {
                return NotFound();
            }

            // Başlık değiştiyse ya da adres eksik yazıldıysa doğru adrese yönlendir.
            if (slug != news.Slug)
            {
                return RedirectToActionPermanent(nameof(Detail), new { id = news.Id, slug = news.Slug });
            }

            await IncreaseViewCountAsync(news.Id);
            news.ViewCount++;

            var model = new NewsDetailViewModel
            {
                News = news,
                Comments = await _commentRepository.Query()
                    .AsNoTracking()
                    .Include(c => c.User)
                    .Where(c => c.NewsId == id && c.IsApproved)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync(),
                Related = await _newsRepository.Published()
                    .Where(n => n.CategoryId == news.CategoryId && n.Id != news.Id)
                    .OrderByDescending(n => n.PublishedAt)
                    .Take(3)
                    .ToListAsync(),
                MostRead = await _newsRepository.Published()
                    .Where(n => n.Id != news.Id)
                    .OrderByDescending(n => n.ViewCount)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        // GET: /kategori/ekonomi?page=2
        public async Task<IActionResult> ByCategory(string slug, int page = 1)
        {
            var category = await _categoryRepository.GetFirstAsync(c => c.Slug == slug && c.IsActive);
            if (category == null)
            {
                return NotFound();
            }

            var query = _newsRepository.Published().Where(n => n.CategoryId == category.Id);
            var model = await BuildArchiveAsync(query, page);
            model.Title = category.Name;
            model.Description = category.Description;
            model.Kind = "category";
            model.Slug = category.Slug;

            return View("Archive", model);
        }

        // GET: /etiket/yapay-zeka
        public async Task<IActionResult> ByTag(string slug, int page = 1)
        {
            var tag = await _tagRepository.GetFirstAsync(t => t.Slug == slug);
            if (tag == null)
            {
                return NotFound();
            }

            var query = _newsRepository.Published().Where(n => n.NewsTags.Any(nt => nt.TagId == tag.Id));
            var model = await BuildArchiveAsync(query, page);
            model.Title = "#" + tag.Name;
            model.Description = "Bu etiketle yayımlanan haberler";
            model.Kind = "tag";
            model.Slug = tag.Slug;

            return View("Archive", model);
        }

        // GET: /ara?q=turizm
        public async Task<IActionResult> Search(string? q, int page = 1)
        {
            var term = q?.Trim() ?? string.Empty;
            var query = term.Length >= 2
                ? _newsRepository.Published().Where(n => n.Title.Contains(term) || n.Summary.Contains(term) || n.Content.Contains(term))
                : _newsRepository.Published().Where(n => false);

            var model = await BuildArchiveAsync(query, page);
            model.Title = term.Length >= 2 ? $"\"{term}\" için sonuçlar" : "Haberlerde ara";
            model.Description = term.Length >= 2 ? $"{model.TotalCount} haber bulundu" : "Aramak için en az 2 karakter yazın.";
            model.Kind = "search";
            model.Query = term;

            return View("Archive", model);
        }

        private async Task<NewsArchiveViewModel> BuildArchiveAsync(IQueryable<News> query, int page)
        {
            if (page < 1) page = 1;

            var (items, total) = await _newsRepository.GetPublishedPageAsync(query, page, PageSize);

            return new NewsArchiveViewModel
            {
                Items = items,
                Page = page,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize),
                MostRead = await _newsRepository.Published()
                    .OrderByDescending(n => n.ViewCount)
                    .Take(5)
                    .ToListAsync()
            };
        }

        private async Task IncreaseViewCountAsync(int newsId)
        {
            // Aynı ziyaretçi sayfayı yenilediğinde sayaç tekrar artmasın diye kısa ömürlü bir cookie kullanılır.
            var cookieName = $"read_{newsId}";
            if (Request.Cookies.ContainsKey(cookieName))
            {
                return;
            }

            var tracked = await _newsRepository.GetByIdAsync(newsId);
            if (tracked != null)
            {
                tracked.ViewCount++;
                await _newsRepository.SaveAsync();
            }

            Response.Cookies.Append(cookieName, "1", new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddHours(6),
                HttpOnly = true,
                IsEssential = true
            });
        }
    }
}
