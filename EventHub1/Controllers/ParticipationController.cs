using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EventHub1.Data;
using EventHub1.Models;
using Microsoft.EntityFrameworkCore;

namespace EventHub1.Controllers
{
    [Authorize]
    public class ParticipationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ParticipationController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Join(int eventId, string status)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventItem == null) return NotFound();

            // 🔥 aktuelle Teilnehmer zählen (nur "Going")
            var countGoing = await _context.Participations
                .CountAsync(p => p.EventId == eventId && p.Status == "Going");

            // ❗ Limit prüfen
            if (status == "Going" && countGoing >= eventItem.MaxParticipants)
            {
                return BadRequest("Event ist voll!");
            }

            var existing = await _context.Participations
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);

            if (existing == null)
            {
                _context.Participations.Add(new Participation
                {
                    EventId = eventId,
                    UserId = userId,
                    Status = status
                });
            }
            else
            {
                existing.Status = status;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Events", new { id = eventId });
        }
    }
}
