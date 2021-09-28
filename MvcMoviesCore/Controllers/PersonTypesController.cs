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
        // GET: PersonTypesController
        public async Task<ActionResult> Index()
        {
            using (var db = new MvcMovieCoreContext())
            {
                IQueryable<PersonTypes> personTypes = db.PersonType.Include(i => i.Person);
                return View(await personTypes.ToListAsync());
            }
        }

        // GET: PersonTypesController/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            using (var db = new MvcMovieCoreContext())
            {
                IQueryable<PersonTypes> personTypes;
                if (id.HasValue)
                    personTypes = db.PersonType.Include(i => i.Person).Where(f => f.Id == id);
                else
                    personTypes = db.PersonType.Include(i => i.Person);
                return View(await personTypes.ToListAsync());
            }
        }

        // GET: PersonTypesController/Create
        public ActionResult Create()
        {
            using (var ctx = new MvcMovieCoreContext())
            {
                ViewData["PersonId"] = new SelectList(ctx.Person, "Id", "Name");
                return View();
            }
        }

        // POST: Person/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Id,Name,PersonId")] PersonTypes personTypes)
        {
            using (var ctx = new MvcMovieCoreContext())
            {
                if (ModelState.IsValid)
                {
                    personTypes.Id = Guid.NewGuid();
                    ctx.Add(personTypes);
                    await ctx.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["PersonId"] = new SelectList(ctx.Person, "Id", "Name");
                return View();
            }
        }

        // GET: PersonTypesController/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (!id.HasValue)
                return NotFound();

            using (var ctx = new MvcMovieCoreContext())
            {
                var personTypes = await ctx.PersonType.Include(i => i.Person).FirstOrDefaultAsync(f => f.Id == id);
                if (personTypes == null)
                    return NotFound();

                ViewData["PersonId"] = new SelectList(ctx.Person, "Id", "Name");
                return View(personTypes);
            }
        }

        // POST: PersonTypesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, [Bind("Id,Name,PersonId")] PersonTypes personTypes)
        {
            if (id != personTypes.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    using (var ctx = new MvcMovieCoreContext())
                    {
                        ctx.Update(personTypes);
                        await ctx.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonTypeExists(personTypes.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["PersonId"] = new SelectList(new MvcMovieCoreContext().Person, "Id", "Name");
            return View(personTypes);
        }

        // GET: PersonTypesController/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (!id.HasValue)
                return NotFound();

            using (MvcMovieCoreContext ctx = new MvcMovieCoreContext())
            {
                var personType = await ctx.PersonType.Include(i => i.Person).FirstOrDefaultAsync(f => f.Id == id);

                if (personType == null)
                    return NotFound();

                ViewData["PersonId"] = new SelectList(ctx.Person, "Id", "Name"); 
                return View(personType);
            }
        }

        // POST: PersonTypesController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            using (var ctx = new MvcMovieCoreContext())
            {
                var personType = await ctx.PersonType.FindAsync(id);
                ctx.PersonType.Remove(personType);
                await ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        private bool PersonTypeExists(Guid id)
        {
            using (var ctx = new MvcMovieCoreContext())
                return ctx.PersonType.Any(a => a.Id == id);
        }
    }
}