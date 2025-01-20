using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using System;
using System.Linq;

namespace MvcMoviesCore.Controllers
{
    public class MovieRoleController : Controller
    {
        #region privat fields

        private readonly MvcMovieCoreContext _context;

        #endregion

        #region constructor

        public MovieRoleController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        #endregion

        #region public functions

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(Guid id)
        {
            var role = _context.MovieRole.FirstOrDefault(x => x.Id.Equals(id));
            if (role == null)
                return NotFound();
            var moviePersons = _context.MoviesPerson
                .Include(i => i.Movies)
                .Include(i => i.Person)
                .Where(w => w.MovieRole.Id.Equals(id))
                .OrderBy(o=>o.Person.Name)
                .ThenBy(o => o.Movies.Name)
                .ToList();
            return View(moviePersons);
        }


        #endregion

        #region private functions

        #endregion
    }
}