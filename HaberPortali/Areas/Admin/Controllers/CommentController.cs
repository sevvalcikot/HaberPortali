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
    public class CommentController : Controller
    {
        private readonly IRepository<Comment> _commentRepository;

        public CommentController(IRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        // GET: /Admin/Comment?status=pending|approved|all
        public async Task<IActionResult> Index(string status = "pending")
        {
            var query = _commentRepository.Query()
                .AsNoTracking()
                .Include(c => c.News)
                .Include(c => c.User)
                .AsQueryable();

            query = status switch
            {
                "approved" => query.Where(c => c.IsApproved),
                "all" => query,
                _ => query.Where(c => !c.IsApproved)
            };

            var model = new CommentListViewModel
            {
                Status = status is "approved" or "all" ? status : "pending",
                Items = await query.OrderByDescending(c => c.CreatedAt).Take(200).ToListAsync(),
                PendingCount = await _commentRepository.CountAsync(c => !c.IsApproved),
                ApprovedCount = await _commentRepository.CountAsync(c => c.IsApproved)
            };

            return View(model);
        }

        // AJAX: yorumu onaylar ya da onayını kaldırır.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleApprove(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                return Json(new { success = false, message = "Yorum bulunamadı." });
            }

            comment.IsApproved = !comment.IsApproved;
            await _commentRepository.SaveAsync();

            return Json(new
            {
                success = true,
                value = comment.IsApproved,
                message = comment.IsApproved ? "Yorum onaylandı, artık sitede görünüyor." : "Yorumun onayı kaldırıldı.",
                pendingCount = await _commentRepository.CountAsync(c => !c.IsApproved)
            });
        }

        // AJAX: yorumu siler.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                return Json(new { success = false, message = "Yorum bulunamadı." });
            }

            _commentRepository.Delete(comment);
            await _commentRepository.SaveAsync();

            return Json(new
            {
                success = true,
                message = "Yorum silindi.",
                pendingCount = await _commentRepository.CountAsync(c => !c.IsApproved)
            });
        }
    }
}
