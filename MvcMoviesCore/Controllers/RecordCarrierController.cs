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
    public class RecordCarrierController : Controller
    {
        #region fields

        private readonly MvcMovieCoreContext _context;
        private readonly IConfiguration _configuration;

        #endregion

        #region contructor

        public RecordCarrierController(MvcMovieCoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            var recordCarrier = await _context.RecordCarrier.FindAsync(id);
            if (recordCarrier == null)
            {
                return NotFound();
            }
            return View(recordCarrier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description")] RecordCarrier recordCarrier)
        {
            if (id != recordCarrier.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recordCarrier);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecordCarrierExists(recordCarrier.Id))
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
            return View(recordCarrier);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] RecordCarrier recordCarrier)
        {
            if (ModelState.IsValid)
            {
                recordCarrier.Id = Guid.NewGuid();
                _context.Add(recordCarrier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(recordCarrier);
        }

        #endregion

        #region private functions

        private bool RecordCarrierExists(Guid id)
        {
            return _context.RecordCarrier.Any(x => x.Id == id);
        }

        #endregion
    }
}
