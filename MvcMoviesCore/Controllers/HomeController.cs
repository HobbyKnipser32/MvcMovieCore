using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using System.Diagnostics;
using System.Linq;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace MvcMoviesCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly MvcMovieCoreContext _context;
        private readonly IConfiguration configuration;
        private readonly bool showAdult;

        public HomeController(MvcMovieCoreContext context, IConfiguration configuration)
        {
            _context = context;
            this.configuration = configuration;
            showAdult = this.configuration.GetValue<bool>("AppSettings:ShowAdult");
        }

        public IActionResult Index()
        {
            IQueryable<Movies> movies;

            if (showAdult)
                movies = _context.Movies
                    .Include(m => m.Genre)
                    .Include(m => m.RecordCarrier)
                    .Include(m => m.StorageLocation)
                    .Where(w => w.LastView == null)
                    .OrderByDescending(o => o.CreateDate)
                    .ThenBy(t => t.Name)
                    .Take(20)
                    .AsQueryable();
            else
                movies = _context.Movies
                    .Include(i => i.RecordCarrier)
                    .Include(i => i.StorageLocation)
                    .Include(i => i.Genre)
                    .Where(w => w.Adult.Value == false && w.LastView == null)
                    .OrderByDescending(o => o.CreateDate)
                    .ThenBy(t => t.Name)
                    .Take(20)
                    .AsQueryable();

            return View(movies);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}