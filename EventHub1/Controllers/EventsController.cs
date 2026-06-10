using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHub1.Data;
using EventHub1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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

        // ===================== INDEX =====================
        public async Task<IActionResult> Index(string sortOrder)
{
    var events = _context.Events.AsQueryable();

    if (sortOrder == "old")
        events = events.OrderBy(e => e.Date);
    else
        events = events.OrderByDescending(e => e.Date);

    ViewBag.Categories = new List<string>
    {
        "Party",
        "Gaming",
        "Kino",
        "Sport",
        "Musik",
        "Lernen",
        "Reisen",
        "Food"
    };

    return View(await events.ToListAsync());
}

        // ===================== DETAILS =====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
                return NotFound();

            // 👤 OWNER NAME
            var owner = await _userManager.FindByIdAsync(eventItem.OwnerId);
            ViewBag.OwnerName = owner?.UserName ?? "Unbekannt";

            // 📊 PARTICIPATIONS
            var participations = _context.Participations
                .Where(p => p.EventId == id);

            var going = await participations.CountAsync(p => p.Status == "Going");
            var maybe = await participations.CountAsync(p => p.Status == "Maybe");
            var no = await participations.CountAsync(p => p.Status == "No");

            ViewBag.Going = going;
            ViewBag.Maybe = maybe;
            ViewBag.No = no;

            // 🚨 EVENT FULL CHECK
            ViewBag.IsFull = eventItem.MaxParticipants > 0
                && going >= eventItem.MaxParticipants;

            // 💬 COMMENTS
            ViewBag.Comments = await _context.Comments
                .Where(c => c.EventId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(eventItem);
        }

        // ===================== CREATE (GET) =====================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem)
        {
            // Diagnostic logs to help identify why POST may not create an event
            System.Diagnostics.Debug.WriteLine("CREATE HIT");
            Console.WriteLine("CREATE HIT");

            // Log ModelState validity and any errors
            Console.WriteLine($"ModelState.IsValid = {ModelState.IsValid}");
            foreach (var kv in ModelState)
            {
                if (kv.Value.Errors.Any())
                {
                    Console.WriteLine($"ModelState[{kv.Key}] errors: {string.Join(';', kv.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            if (!ModelState.IsValid)
            {
                // return the view with the model so validation messages are shown
                return View(eventItem);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Console.WriteLine("No authenticated user found during Create POST.");
                return Challenge();
            }

            eventItem.OwnerId = user.Id;

            if (string.IsNullOrEmpty(eventItem.ImageUrl))
            {
                eventItem.ImageUrl = "https://images.unsplash.com/photo-1521737604893";
            }

            try
            {
                _context.Add(eventItem);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log exception and re-show the view with a ModelState error
                Console.WriteLine("Exception saving event: " + ex);
                ModelState.AddModelError(string.Empty, "Beim Speichern des Events ist ein Fehler aufgetreten.");
                return View(eventItem);
            }

            return RedirectToAction(nameof(Index));
        }
        // ===================== EDIT (GET) =====================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge();

            if (eventItem.OwnerId != userId)
                return Forbid();

            return View(eventItem);
        }

        // ===================== EDIT (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventItem)
        {
            if (id != eventItem.Id)
                return NotFound();

            var eventFromDb = await _context.Events.FindAsync(id);

            if (eventFromDb == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge();

            if (eventFromDb.OwnerId != userId)
                return Forbid();

            eventFromDb.Title = eventItem.Title;
            eventFromDb.Description = eventItem.Description;
            eventFromDb.Date = eventItem.Date;
            eventFromDb.Location = eventItem.Location;
            eventFromDb.MaxParticipants = eventItem.MaxParticipants;
            eventFromDb.Category = eventItem.Category;
            eventFromDb.ImageUrl = eventItem.ImageUrl;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===================== DELETE (GET) =====================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge();

            if (eventItem.OwnerId != userId)
                return Forbid();

            return View(eventItem);
        }

        // ===================== DELETE (POST) =====================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge();

            if (eventItem.OwnerId != userId)
                return Forbid();

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===================== CATEGORY FILTER =====================
        public async Task<IActionResult> ByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return RedirectToAction(nameof(Index));

            var events = await _context.Events
                .Where(e => e.Category == category)
                .AsNoTracking()
                .ToListAsync();

            return View("Index", events);
        }
    }
}