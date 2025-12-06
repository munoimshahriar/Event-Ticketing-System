using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketing.Data;
using VirtualEventTicketing.Models;

namespace VirtualEventTicketing.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index(EventFilterViewModel? filter, bool isAjax = false)
        {
            var viewModel = filter ?? new EventFilterViewModel();
            
            // Start with base query
            var query = _context.Events.Include(e => e.Category).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(viewModel.SearchTitle))
            {
                query = query.Where(e => e.Title.Contains(viewModel.SearchTitle));
            }

            // Apply category filter
            if (viewModel.CategoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == viewModel.CategoryId.Value);
            }

            // Apply date range filters
            if (viewModel.DateFrom.HasValue)
            {
                query = query.Where(e => e.Date >= viewModel.DateFrom.Value);
            }

            if (viewModel.DateTo.HasValue)
            {
                query = query.Where(e => e.Date <= viewModel.DateTo.Value);
            }

            // Apply availability filter
            if (viewModel.Availability == "available")
            {
                query = query.Where(e => e.AvailableTickets > 0);
            }
            else if (viewModel.Availability == "soldout")
            {
                query = query.Where(e => e.AvailableTickets == 0);
            }

            // Apply sorting
            query = viewModel.SortBy switch
            {
                "title" => query.OrderBy(e => e.Title),
                "price" => query.OrderBy(e => e.TicketPrice),
                "date" => query.OrderBy(e => e.Date),
                _ => query.OrderBy(e => e.Date)
            };

            viewModel.Events = await query.ToListAsync();
            viewModel.Categories = await _context.Categories.ToListAsync();

            // Return partial view for AJAX requests
            if (isAjax)
            {
                return PartialView("_EventListPartial", viewModel.Events);
            }

            return View(viewModel);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        [Authorize(Policy = "OrganizerOrAdmin")]
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

// POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "OrganizerOrAdmin")]
        public async Task<IActionResult> Create([Bind("Title,Date,TicketPrice,AvailableTickets,CategoryId")] Event @event)
        {
            // Remove validation for navigation properties that aren't in the form
            ModelState.Remove("Category");
            ModelState.Remove("Organizer");

            if (ModelState.IsValid)
            {
                // Set OrganizerId to current user
                @event.OrganizerId = User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;

                // Ensure DateTime is UTC for PostgreSQL
                if (@event.Date.Kind == DateTimeKind.Unspecified)
                {
                    @event.Date = DateTime.SpecifyKind(@event.Date, DateTimeKind.Utc);
                }
                else if (@event.Date.Kind == DateTimeKind.Local)
                {
                    @event.Date = @event.Date.ToUniversalTime();
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we get here, something failed. Reload the category list.
            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", @event.CategoryId);
            return View(@event);
        }


        // GET: Events/Edit/5
        [Authorize(Policy = "OrganizerOrAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            // Check if user is the organizer or admin
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (@event.OrganizerId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", @event.CategoryId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "OrganizerOrAdmin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Date,TicketPrice,AvailableTickets,CategoryId")] Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            // Check if user is the organizer or admin
            var existingEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (existingEvent == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (existingEvent.OrganizerId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            // Remove validation for navigation properties that aren't in the form
            ModelState.Remove("Category");
            ModelState.Remove("Organizer");

            if (ModelState.IsValid)
            {
                // Preserve OrganizerId
                @event.OrganizerId = existingEvent.OrganizerId;

                // Ensure DateTime is UTC for PostgreSQL
                if (@event.Date.Kind == DateTimeKind.Unspecified)
                {
                    @event.Date = DateTime.SpecifyKind(@event.Date, DateTimeKind.Utc);
                }
                else if (@event.Date.Kind == DateTimeKind.Local)
                {
                    @event.Date = @event.Date.ToUniversalTime();
                }

                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
    
            // If validation fails, reload the dropdown
            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", @event.CategoryId);
            return View(@event);
        }

        // GET: Events/Delete/5
        [Authorize(Policy = "OrganizerOrAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (@event == null)
            {
                return NotFound();
            }

            // Check if user is the organizer or admin
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (@event.OrganizerId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "OrganizerOrAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            // Check if user is the organizer or admin
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (@event.OrganizerId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalEvents = await _context.Events.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var lowAvailabilityEvents = await _context.Events
                .Include(e => e.Category)
                .Where(e => e.AvailableTickets < 5)
                .ToListAsync();

            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.LowAvailabilityEvents = lowAvailabilityEvents;

            return View();
        }

        // GET: Events/MyAnalytics
        [Authorize(Roles = "Admin,Organizer")]
        public IActionResult MyAnalytics()
        {
            return View();
        }

        // GET: Events/GetAnalyticsData
        [HttpGet]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> GetAnalyticsData()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Ticket Sales by Category
            var salesByCategoryQuery = _context.PurchaseItems
                .Include(pi => pi.Event)
                    .ThenInclude(e => e.Category)
                .Include(pi => pi.Purchase)
                .AsQueryable();

            // Filter by organizer if not admin
            if (!isAdmin && currentUserId != null)
            {
                salesByCategoryQuery = salesByCategoryQuery.Where(pi => pi.Event.OrganizerId == currentUserId);
            }

            var salesByCategory = await salesByCategoryQuery
                .GroupBy(pi => pi.Event.Category.Name)
                .Select(g => new { 
                    category = g.Key, 
                    count = g.Sum(x => x.Quantity) 
                })
                .ToListAsync();

            // Revenue per Month
            var revenueByMonthQuery = _context.Purchases
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Event)
                .AsQueryable();

            // Filter by organizer if not admin
            if (!isAdmin && currentUserId != null)
            {
                revenueByMonthQuery = revenueByMonthQuery.Where(p => 
                    p.PurchaseItems.Any(pi => pi.Event.OrganizerId == currentUserId));
            }

            var revenueByMonth = await revenueByMonthQuery
                .GroupBy(p => new { 
                    Year = p.PurchaseDate.Year, 
                    Month = p.PurchaseDate.Month 
                })
                .Select(g => new { 
                    month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    revenue = g.Sum(p => p.TotalCost)
                })
                .OrderBy(x => x.month)
                .ToListAsync();

            // Top 5 Best-Selling Events
            var topEventsQuery = _context.PurchaseItems
                .Include(pi => pi.Event)
                .Include(pi => pi.Purchase)
                .GroupBy(pi => new { pi.EventId, pi.Event.Title })
                .Select(g => new {
                    eventId = g.Key.EventId,
                    eventTitle = g.Key.Title,
                    ticketsSold = g.Sum(x => x.Quantity),
                    revenue = g.Sum(x => x.Purchase.TotalCost)
                })
                .AsQueryable();

            // Filter by organizer if not admin
            if (!isAdmin && currentUserId != null)
            {
                var organizerEventIds = await _context.Events
                    .Where(e => e.OrganizerId == currentUserId)
                    .Select(e => e.Id)
                    .ToListAsync();
                
                topEventsQuery = topEventsQuery.Where(t => organizerEventIds.Contains(t.eventId));
            }

            var topEvents = await topEventsQuery
                .OrderByDescending(x => x.ticketsSold)
                .Take(5)
                .ToListAsync();

            return Json(new { 
                salesByCategory, 
                revenueByMonth,
                topEvents
            });
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}

