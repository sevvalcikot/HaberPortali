using HaberPortali.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaberPortali.ViewComponents
{
    // Sayfanın üstündeki kayan "Son dakika" bandı.
    public class BreakingNewsViewComponent : ViewComponent
    {
        private readonly INewsRepository _newsRepository;

        public BreakingNewsViewComponent(INewsRepository newsRepository)
        {
            _newsRepository = newsRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await _newsRepository.Published()
                .Where(n => n.IsBreaking)
                .OrderByDescending(n => n.PublishedAt)
                .Take(8)
                .ToListAsync();

            return View(items);
        }
    }
}
