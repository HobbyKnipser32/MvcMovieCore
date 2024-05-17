using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace MvcMoviesCore.Controllers
{
    public class SexController : Controller
    {
        #region fields

        private readonly MvcMovieCoreContext _context;

        #endregion

        #region contructor

        public SexController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        #endregion

        #region public functions
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sex = await _context.Sex.FindAsync(id);
            if (sex == null)
            {
                return NotFound();
            }
            return View(sex);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description")] Sex sex)
        {
            if (id != sex.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sex);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SexExists(sex.Id))
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
            return View(sex);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Sex sex)
        {
            if (ModelState.IsValid)
            {
                sex.Id = Guid.NewGuid();
                _context.Add(sex);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sex);
        }

        #endregion

        #region private functions

        private bool SexExists(Guid id)
        {
            return _context.Sex.Any(x => x.Id == id);
        }

        #endregion
    }
}
