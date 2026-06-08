using EventHub1.Data;
using EventHub1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub1.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string id)
        {
            var userId = id ?? _userManager.GetUserId(User);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var events = await _context.Events
                .Where(e => e.OwnerId == userId)
                .ToListAsync();

            var comments = await _context.Comments
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var model = new UserProfileViewModel
            {
                UserId = userId,
                UserName = user.UserName,
                Events = events,
                Comments = comments
            };

            return View(model);
        }
    }
}
