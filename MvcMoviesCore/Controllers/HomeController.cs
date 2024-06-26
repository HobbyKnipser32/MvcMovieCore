using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using MvcMoviesCore.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MvcMoviesCore.Controllers
{
    public class HomeController : Controller
    {
        #region privat fields

        private readonly MvcMovieCoreContext _context;
        private readonly IConfiguration _configuration;
        private readonly bool _showAdult;
        private readonly string _originalFileDirectory = "Images/Original";

        #endregion

        #region constructor

        public HomeController(MvcMovieCoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _showAdult = _configuration.GetValue<bool>("AppSettings:ShowAdult");
            _originalFileDirectory = _configuration.GetValue<string>("AppSettings:OriginalFileDirectory");
        }

        #endregion

        #region public functions

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

        public IActionResult Search(string searchText)
        {
            var model = new List<SearchResult>();
            model.AddRange(SearchMovies(searchText));
            model.AddRange(SearchPersons(searchText));
            return View(model.OrderBy(o => o.Name).ToList());
        }

        #endregion

        #region private functions

        private List<SearchResult> SearchMovies(string searchText)
        {
            var model = new List<SearchResult>();
            var movies = _context.Movies.Where(w => w.Name.Contains(searchText)).ToList();
            foreach (var movie in movies)
            {
                var result = new SearchResult() { Id = movie.Id, Name = movie.Name, TypeOf = "Movies" };
                model.Add(result);
            }
            return model;
        }

        private List<SearchResult> SearchPersons(string searchText)
        {
            var model = new List<SearchResult>();
            var persons = _context.Person.Where(w => w.Name.Contains(searchText)).ToList();
            foreach (var person in persons)
            {
                var result = new SearchResult() { Name = person.Name, Id = person.Id, TypeOf = "Person" };
                model.Add(result);
            }
            return model;
        }


        #endregion
    }
}