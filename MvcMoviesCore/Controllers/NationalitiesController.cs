using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            if (nationalities.Count != 0)
            {
                var persons = _context.Person.GroupBy(g => g.NationalityId).ToList();
                foreach (var person in persons)
                {
                    var nationality = nationalities.FirstOrDefault(f => f.Id.Equals(person.Key));
                    if (nationality != null)
                        nationality.Count = person.Count();
                }
            }
            return View(nationalities);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nationality = await _context.Nationalities.FindAsync(id);
            if (nationality == null)
            {
                return NotFound();
            }
            return View(nationality);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description")] Nationality nationality)
        {
            if (id != nationality.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nationality);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NationalityExists(nationality.Id))
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
            return View(nationality);
        }

        #region private functions

        private bool NationalityExists(Guid id)
        {
            return _context.Nationalities.Any(x => x.Id == id);
        }

        #endregion
    }
}
