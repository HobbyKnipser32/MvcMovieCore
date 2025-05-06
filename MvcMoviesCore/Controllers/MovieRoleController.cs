using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.Controllers
{
    public class MovieRoleController(MvcMovieCoreContext context) : Controller
    {
        #region privat fields

        private readonly MvcMovieCoreContext _context = context;

        #endregion
        
        #region constructor

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
                .OrderBy(o => o.Person.Name)
                .ThenBy(o => o.Movies.Name)
                .ToList();
            return View(moviePersons);
        }

        public IActionResult Edit(Guid id)
        {
            var role = _context.MovieRole.FirstOrDefault(f => f.Id.Equals(id));
            if (role == null)
                return NotFound();
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,IsAdult")] MovieRole role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieRoleExists(role.Id))
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
            return View(role);
        }


        #endregion

        #region private functions
        private bool MovieRoleExists(Guid id)
        {
            return _context.MovieRole.Any(e => e.Id == id);
        }

        #endregion
    }
}