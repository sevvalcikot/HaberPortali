using HaberPortali.Data;
using HaberPortali.Helpers;
using HaberPortali.Models;
using Microsoft.EntityFrameworkCore;

namespace HaberPortali.Repositories
{
    public class NewsRepository : Repository<News>, INewsRepository
    {
        public NewsRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<News?> GetDetailAsync(int id)
        {
            return await _dbSet
                .Include(n => n.Category)
                .Include(n => n.Author)
                .Include(n => n.NewsTags).ThenInclude(nt => nt.Tag)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<(List<News> Items, int TotalCount)> GetPagedAsync(string? search, int? categoryId, int page, int pageSize, bool onlyPublished)
        {
            IQueryable<News> query = _dbSet
                .AsNoTracking()
                .Include(n => n.Category)
                .Include(n => n.Author);

            if (onlyPublished)
            {
                var now = DateTime.Now;
                query = query.Where(n => n.IsActive && n.PublishedAt <= now);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(n => n.Title.Contains(term) || n.Summary.Contains(term));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(n => n.CategoryId == categoryId.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(n => n.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public IQueryable<News> Published()
        {
            var now = DateTime.Now;
            return _dbSet
                .AsNoTracking()
                .Include(n => n.Category)
                .Where(n => n.IsActive && n.Category.IsActive && n.PublishedAt <= now);
        }

        public async Task<(List<News> Items, int TotalCount)> GetPublishedPageAsync(IQueryable<News> query, int page, int pageSize)
        {
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(n => n.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<List<News>> GetLatestAsync(int count)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(n => n.Category)
                .OrderByDescending(n => n.PublishedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<News>> GetMostReadAsync(int count)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(n => n.Category)
                .OrderByDescending(n => n.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task SetTagsAsync(News news, string? tagsText)
        {
            var names = (tagsText ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(n => n.Length <= 50)
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            // Veritabanında zaten olan etiketler tekrar oluşturulmaz.
            var existingTags = await _context.Tags
                .Where(t => names.Contains(t.Name))
                .ToListAsync();

            var targetTags = new List<Tag>();
            foreach (var name in names)
            {
                var tag = existingTags.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.CurrentCultureIgnoreCase));
                if (tag == null)
                {
                    tag = new Tag { Name = name, Slug = SlugHelper.ToSlug(name) };
                    _context.Tags.Add(tag);
                    existingTags.Add(tag);
                }
                targetTags.Add(tag);
            }

            // Artık listede olmayan etiket bağlantılarını kaldır.
            var toRemove = news.NewsTags
                .Where(nt => !targetTags.Any(t => t.Id != 0 && t.Id == nt.TagId))
                .ToList();

            foreach (var newsTag in toRemove)
            {
                news.NewsTags.Remove(newsTag);
                _context.NewsTags.Remove(newsTag);
            }

            // Yeni etiket bağlantılarını ekle.
            foreach (var tag in targetTags)
            {
                var alreadyLinked = tag.Id != 0 && news.NewsTags.Any(nt => nt.TagId == tag.Id);
                if (!alreadyLinked)
                {
                    news.NewsTags.Add(new NewsTag { News = news, Tag = tag });
                }
            }
        }
    }
}
