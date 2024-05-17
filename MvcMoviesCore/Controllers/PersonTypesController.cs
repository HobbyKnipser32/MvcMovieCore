using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.Controllers
{
    public class PersonTypesController : Controller
    {
        #region fields

        private readonly MvcMovieCoreContext _context;

        #endregion

        #region constructor

        public PersonTypesController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        #endregion

        #region public functions

        // GET: PersonTypesController
        public ActionResult Index()
        {
            return View();
            //using (var db = new MvcMovieCoreContext())
            //{
            //    IQueryable<PersonTypes> personType = db.PersonType.Include(i => i.Person);
            //    return View(await personType.ToListAsync());
            //}
        }

        //// GET: PersonTypesController/Details/5
        //public async Task<ActionResult> Details(Guid? id)
        //{
        //    using (var db = new MvcMovieCoreContext())
        //    {
        //        IQueryable<PersonTypes> personType;
        //        if (id.HasValue)
        //            personType = db.PersonType.Include(i => i.Person).Where(f => f.Id == id);
        //        else
        //            personType = db.PersonType.Include(i => i.Person);
        //        return View(await personType.ToListAsync());
        //    }
        //}

        // GET: PersonTypesController/Create
        public ActionResult Create()
        {
            return View();
            //using (var ctx = new MvcMovieCoreContext())
            //{
            //    ViewData["PersonId"] = new SelectList(ctx.Person, "Id", "Name");
            //    return View();
            //}
        }

        // POST: Person/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Name,Description")] PersonTypes personType)
        {
            if (ModelState.IsValid)
            {
                personType.Id = Guid.NewGuid();
                _context.Add(personType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(personType);
        }

        // GET: PersonTypesController/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (!id.HasValue)
                return NotFound();

            var personTypes = await _context.PersonType.Include(i => i.Person).FirstOrDefaultAsync(f => f.Id == id);
            if (personTypes == null)
                return NotFound();

            return View(personTypes);
        }

        // POST: PersonTypesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, [Bind("Id,Name,Description")] PersonTypes personType)
        {
            if (id != personType.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                        _context.Update(personType);
                        await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonTypeExists(personType.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["PersonId"] = new SelectList(new MvcMovieCoreContext().Person, "Id", "Name");
            return View(personType);
        }

        //// GET: PersonTypesController/Delete/5
        //public async Task<ActionResult> Delete(Guid? id)
        //{
        //    if (!id.HasValue)
        //        return NotFound();

        //    using (MvcMovieCoreContext ctx = new MvcMovieCoreContext())
        //    {
        //        var personType = await ctx.PersonType.Include(i => i.Person).FirstOrDefaultAsync(f => f.Id == id);

        //        if (personType == null)
        //            return NotFound();

        //        ViewData["PersonId"] = new SelectList(ctx.Person, "Id", "Name");
        //        return View(personType);
        //    }
        //}

        //// POST: PersonTypesController/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(Guid id)
        //{
        //    using (var ctx = new MvcMovieCoreContext())
        //    {
        //        var personType = await ctx.PersonType.FindAsync(id);
        //        ctx.PersonType.Remove(personType);
        //        await ctx.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        #endregion

        #region private functions

        private bool PersonTypeExists(Guid id)
        {
            return _context.PersonType.Any(a => a.Id == id);
        }

        #endregion
    }
}