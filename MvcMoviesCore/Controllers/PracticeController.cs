using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Data;
using MvcMoviesCore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.Controllers
{
    public class PracticeController : Controller
    {
        #region fields

        private readonly MvcMovieCoreContext _context;

        #endregion

        #region constructor

        public PracticeController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        #endregion

        #region puclic functions


        public async Task<IActionResult> Index() => View();

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Praxis,Description,IsAdult")] Practice practice)
        {
            if (ModelState.IsValid)
            {
                _context.Add(practice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(practice);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var practice = await _context.Practices.FirstOrDefaultAsync(f => f.Id.Equals(id));
            if (practice == null)
            {
                return NotFound();
            }
            return View(practice);
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Praxis,Description,IsAdult")] Practice practice)
        {
            if (id != practice.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(practice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(practice);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var practice = await _context.Practices.FirstOrDefaultAsync(m => m.Id == id);
            if (practice == null)
            {
                return NotFound();
            }

            return View(practice);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var practice = await _context.Practices.FindAsync(id);
            _context.Practices.Remove(practice);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}