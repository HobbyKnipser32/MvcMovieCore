using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using System;
using System.Diagnostics;
using System.Linq;
//using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace MvcMoviesCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly MvcMovieCoreContext _context;
        private readonly IConfiguration _configuration;
        private readonly bool _showAdult;
        private readonly string _originalFileDirectory = "Images/Original";

        public HomeController(MvcMovieCoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _showAdult = _configuration.GetValue<bool>("AppSettings:ShowAdult");
            _originalFileDirectory = _configuration.GetValue<string>("AppSettings:OriginalFileDirectory");
        }

        public IActionResult Index()
        {
            IQueryable<Movies> movies;

            if (_showAdult)
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
                    .Where(w => w.Adult == false && w.LastView == null)
                    .OrderByDescending(o => o.CreateDate)
                    .ThenBy(t => t.Name)
                    .Take(20)
                    .AsQueryable();

            ViewData["ImageSource"] = _originalFileDirectory;
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

        /// <summary>
        /// Sets the language.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>       
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Secure = true }
            );

            return LocalRedirect(returnUrl);
        }
    }
}