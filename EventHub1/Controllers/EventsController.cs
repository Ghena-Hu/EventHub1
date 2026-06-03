using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHub1.Data;
using EventHub1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EventHub1.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EventsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return View(events);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
                return NotFound();

            ViewBag.Going = await _context.Participations
                .CountAsync(p => p.EventId == id && p.Status == "Going");

            ViewBag.Maybe = await _context.Participations
                .CountAsync(p => p.EventId == id && p.Status == "Maybe");

            ViewBag.No = await _context.Participations
                .CountAsync(p => p.EventId == id && p.Status == "No");

            return View(eventItem);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem)
        {
            var userId = _userManager.GetUserId(User);

            eventItem.OwnerId = userId;

            _context.Add(eventItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            if (eventItem.OwnerId != userId)
                return Forbid();

            return View(eventItem);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventItem)
        {
            if (id != eventItem.Id) return NotFound();

            var existingEvent = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existingEvent == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            if (existingEvent.OwnerId != userId)
                return Forbid();

            eventItem.OwnerId = existingEvent.OwnerId;

            if (!ModelState.IsValid)
                return View(eventItem);

            _context.Update(eventItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var eventItem = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            if (eventItem.OwnerId != userId)
                return Forbid();

            return View(eventItem);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            if (eventItem.OwnerId != userId)
                return Forbid();

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}