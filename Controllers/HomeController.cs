using Microsoft.AspNetCore.Mvc;
using VirtualEventTicketing.Data;

namespace VirtualEventTicketing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("User visited the Home Page at {Time}", DateTime.Now);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}

