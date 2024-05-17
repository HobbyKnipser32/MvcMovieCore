using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.Controllers
{
    public class StorageLocationController : Controller
    {
        #region fields

        private readonly MvcMovieCoreContext _context;

        #endregion

        #region contructor

        public StorageLocationController(MvcMovieCoreContext context)
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

            var storageLocation = await _context.StorageLocation.FindAsync(id);
            if (storageLocation == null)
            {
                return NotFound();
            }
            return View(storageLocation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description")] StorageLocation storageLocation)
        {
            if (id != storageLocation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(storageLocation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StorageLocationExists(storageLocation.Id))
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
            return View(storageLocation);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] StorageLocation storageLocation)
        {
            if (ModelState.IsValid)
            {
                storageLocation.Id = Guid.NewGuid();
                _context.Add(storageLocation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(storageLocation);
        }

        #endregion

        #region private functions

        private bool StorageLocationExists(Guid id)
        {
            return _context.StorageLocation.Any(x => x.Id == id);
        }

        #endregion
    }
}
