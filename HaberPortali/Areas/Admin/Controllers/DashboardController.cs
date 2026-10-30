using HaberPortali.Models;
using HaberPortali.Repositories;
using HaberPortali.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaberPortali.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.AdminOrEditor)]
    public class DashboardController : Controller
    {
        private readonly INewsRepository _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Comment> _commentRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Tag> _tagRepository;

        public DashboardController(
            INewsRepository newsRepository,
            IRepository<Category> categoryRepository,
            IRepository<Comment> commentRepository,
            IRepository<User> userRepository,
            IRepository<Tag> tagRepository)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _tagRepository = tagRepository;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                NewsCount = await _newsRepository.CountAsync(),
                ActiveNewsCount = await _newsRepository.CountAsync(n => n.IsActive),
                CategoryCount = await _categoryRepository.CountAsync(),
                TagCount = await _tagRepository.CountAsync(),
                UserCount = await _userRepository.CountAsync(),
                PendingCommentCount = await _commentRepository.CountAsync(c => !c.IsApproved),
                TotalViews = await _newsRepository.Query().SumAsync(n => n.ViewCount),
                LatestNews = await _newsRepository.GetLatestAsync(6),
                MostRead = await _newsRepository.GetMostReadAsync(5),
                PendingComments = await _commentRepository.Query()
                    .AsNoTracking()
                    .Include(c => c.News)
                    .Include(c => c.User)
                    .Where(c => !c.IsApproved)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                CategoryStats = await _categoryRepository.Query()
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new CategoryStat { Name = c.Name, NewsCount = c.News.Count })
                    .ToListAsync()
            };

            return View(model);
        }
    }
}
