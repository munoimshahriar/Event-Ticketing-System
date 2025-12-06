using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketing.Data;
using VirtualEventTicketing.Models;

namespace VirtualEventTicketing.Controllers
{
    [Authorize(Roles = "Attendee,Organizer,Admin")]
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Purchases/Create
        public async Task<IActionResult> Create(int eventId)
        {
            var @event = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (@event == null)
            {
                return NotFound();
            }

            if (@event.AvailableTickets == 0)
            {
                TempData["Error"] = "This event is sold out!";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            var viewModel = new PurchaseViewModel
            {
                EventId = eventId,
                Event = @event
            };

            return View(viewModel);
        }

        // POST: Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseViewModel viewModel)
        {
            var @event = await _context.Events.FindAsync(viewModel.EventId);
            
            if (@event == null)
            {
                return NotFound();
            }

            // Validate quantity
            if (viewModel.Quantity <= 0)
            {
                ModelState.AddModelError("Quantity", "Quantity must be at least 1");
            }

            if (viewModel.Quantity > @event.AvailableTickets)
            {
                ModelState.AddModelError("Quantity", $"Only {@event.AvailableTickets} tickets available");
            }

            if (ModelState.IsValid)
            {
                // Get current user ID if authenticated
                var currentUserId = User.Identity?.IsAuthenticated == true 
                    ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                    : null;

                // Create purchase
                var purchase = new Purchase
                {
                    PurchaseDate = DateTime.UtcNow,
                    TotalCost = @event.TicketPrice * viewModel.Quantity,
                    GuestName = viewModel.GuestName,
                    GuestEmail = viewModel.GuestEmail,
                    UserId = currentUserId // Link to authenticated user if logged in
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                // Create purchase item
                var purchaseItem = new PurchaseItem
                {
                    PurchaseId = purchase.Id,
                    EventId = @event.Id,
                    Quantity = viewModel.Quantity
                };

                _context.PurchaseItems.Add(purchaseItem);

                // Update available tickets
                @event.AvailableTickets -= viewModel.Quantity;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Confirmation), new { id = purchase.Id });
            }

            // Reload event for display
            viewModel.Event = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == viewModel.EventId);

            return View(viewModel);
        }

        // GET: Purchases/Confirmation/5
        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Event)
                        .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }
    }
}

