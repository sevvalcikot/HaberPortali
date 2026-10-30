using HaberPortali.Helpers;
using HaberPortali.Models;
using HaberPortali.Repositories;
using HaberPortali.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaberPortali.Areas.Admin.Controllers
{
    // Kullanıcı yönetimi yalnızca yöneticiye açıktır.
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserController(IRepository<User> userRepository, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.Query()
                .AsNoTracking()
                .Include(u => u.News)
                .OrderBy(u => u.Role).ThenBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserFormViewModel { Role = Roles.Editor, IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Yeni kullanıcı için şifre zorunludur.");
            }

            await ValidateAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                Role = model.Role,
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password!);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            TempData["Success"] = $"{user.FullName} kullanıcısı eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(new UserFormViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var isSelf = id == User.GetUserId();
            if (isSelf && (model.Role != Roles.Admin || !model.IsActive))
            {
                ModelState.AddModelError(string.Empty, "Kendi rolünüzü değiştiremez veya kendi hesabınızı pasif yapamazsınız.");
            }

            await ValidateAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim().ToLowerInvariant();
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
            }

            await _userRepository.SaveAsync();

            TempData["Success"] = "Kullanıcı bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == User.GetUserId())
            {
                TempData["Error"] = "Kendi hesabınızı silemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var hasNews = await _userRepository.Query().Where(u => u.Id == id).SelectMany(u => u.News).AnyAsync();
            var hasComments = await _userRepository.Query().Where(u => u.Id == id).SelectMany(u => u.Comments).AnyAsync();
            if (hasNews || hasComments)
            {
                TempData["Error"] = $"{user.FullName} kullanıcısının haber veya yorumları olduğu için silinemez. Bunun yerine hesabı pasif yapabilirsiniz.";
                return RedirectToAction(nameof(Index));
            }

            _userRepository.Delete(user);
            await _userRepository.SaveAsync();

            TempData["Success"] = "Kullanıcı silindi.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX: hesabı aktif/pasif yapar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (id == User.GetUserId())
            {
                return Json(new { success = false, message = "Kendi hesabınızı pasif yapamazsınız." });
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            user.IsActive = !user.IsActive;
            await _userRepository.SaveAsync();

            return Json(new
            {
                success = true,
                value = user.IsActive,
                message = user.IsActive ? $"{user.FullName} aktif edildi." : $"{user.FullName} pasif yapıldı."
            });
        }

        private async Task ValidateAsync(UserFormViewModel model)
        {
            if (!Roles.All.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Geçersiz rol.");
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var email = model.Email.Trim().ToLowerInvariant();
                if (await _userRepository.AnyAsync(u => u.Email == email && u.Id != model.Id))
                {
                    ModelState.AddModelError(nameof(model.Email), "Bu e-posta adresiyle kayıtlı başka bir kullanıcı var.");
                }
            }
        }
    }
}
