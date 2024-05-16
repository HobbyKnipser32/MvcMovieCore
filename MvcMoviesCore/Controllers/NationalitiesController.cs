using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using System.Linq;

namespace MvcMoviesCore.Controllers
{
    public class NationalitiesController : Controller
    {
        private readonly MvcMovieCoreContext _context;
        private readonly IConfiguration _configuration;

        public NationalitiesController(MvcMovieCoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var nationalities = _context.Nationalities.ToList();
            return View(nationalities);
        }
    }
}
