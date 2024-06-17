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
        #region fields

        private readonly MvcMovieCoreContext _context;
        private readonly string _originalFileDirectory = "/images/original";

        #endregion

        #region constructor

        public NationalitiesController(MvcMovieCoreContext context, IConfiguration configuration)
        {
            _context = context;
            _originalFileDirectory = configuration.GetValue<string>("AppSettings:OriginalFileDirectory");
        }

        #endregion

        #region public functions

        public IActionResult Index()
        {
            ViewData["OriginalFileDirectory"] = _originalFileDirectory;
            return View();
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

        public IActionResult Create()
        {
            return View();
        }

        // POST: Genres/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Nationality nationality)
        {
            if (ModelState.IsValid)
            {
                nationality.Id = Guid.NewGuid();
                _context.Add(nationality);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nationality);
        }
        #endregion

        #region private functions

        private bool NationalityExists(Guid id)
        {
            return _context.Nationalities.Any(x => x.Id == id);
        }

        #endregion
    }
}
