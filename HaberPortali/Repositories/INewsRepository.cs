using HaberPortali.Models;

namespace HaberPortali.Repositories
{
    // Haberlere özel sorgular. Genel işlemler IRepository<News>'ten gelir.
    public interface INewsRepository : IRepository<News>
    {
        // Kategori, yazar ve etiketleriyle birlikte tek haber (değişiklik takibi açık).
        Task<News?> GetDetailAsync(int id);

        Task<(List<News> Items, int TotalCount)> GetPagedAsync(string? search, int? categoryId, int page, int pageSize, bool onlyPublished);

        // Sitede gösterilebilecek haberler: yayında ve yayın tarihi gelmiş olanlar (kategori dahil, salt okunur).
        IQueryable<News> Published();

        Task<(List<News> Items, int TotalCount)> GetPublishedPageAsync(IQueryable<News> query, int page, int pageSize);

        Task<List<News>> GetLatestAsync(int count);

        Task<List<News>> GetMostReadAsync(int count);

        // "ekonomi, enflasyon, merkez bankası" gibi virgülle yazılmış etiketleri habere bağlar.
        Task SetTagsAsync(News news, string? tagsText);
    }
}
