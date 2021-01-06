using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.Controllers
{
    public class PersonController : Controller
    {
        private readonly MvcMovieCoreContext _context;

        public PersonController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        // GET: Person
        public async Task<IActionResult> Index(string filter = null)
        {
            IQueryable<Person> mvcMovieCoreContext = _context.Person.Include(p => p.PersonType).Include(p => p.Sex).Include(p => p.Nationality).OrderBy(o => o.Name);
            if (!string.IsNullOrWhiteSpace(filter))
                mvcMovieCoreContext = mvcMovieCoreContext.Where(w => w.Name.Contains(filter));

            //model.RouteValue = new RouteValueDictionary { { "filter", filter } };

            return View(await mvcMovieCoreContext.ToListAsync());
        }

        // GET: Person/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .Include(p => p.PersonType)
                .Include(p => p.Sex)
                .Include(p => p.Nationality)
                .Include(p => p.MoviesPerson)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            person.ActorsAge = person.GetActorsAge(person.Birthday, person.Obit);

            foreach (var mpId in person.MoviesPerson)
            {
                mpId.Movies = await _context.Movies.FirstOrDefaultAsync(f => f.Id == mpId.MoviesId);
                mpId.Movies.ActorsAge = mpId.Movies.GetActorsMovieAge(person.Birthday, mpId.Movies.YearOfPublication);
            }

            person.MoviesPerson = person.MoviesPerson.OrderBy(o => o.Movies.YearOfPublication).ThenBy(t => t.Movies.Name).ToList();

            return View(person);
        }

        // GET: Person/Create
        public IActionResult Create()
        {
            ViewData["PersonTypesId"] = new SelectList(_context.PersonType, "Id", "Name");
            ViewData["SexId"] = new SelectList(_context.Sex, "Id", "Name");
            return View();
        }

        // POST: Person/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SexId,Birthday,Obit,Nationality,Größe,Gewicht,PersonTypesId")] Person person)
        {
            if (ModelState.IsValid)
            {
                person.Id = Guid.NewGuid();
                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PersonTypesId"] = new SelectList(_context.PersonType, "Id", "Name", person.PersonTypesId);
            ViewData["SexId"] = new SelectList(_context.Sex, "Id", "Name", person.SexId);
            return View(person);
        }

        // GET: Person/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Person.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            ViewData["PersonTypesId"] = new SelectList(_context.PersonType, "Id", "Name", person.PersonTypesId);
            ViewData["SexId"] = new SelectList(_context.Sex, "Id", "Name", person.SexId);
            ViewData["NationalityId"] = new SelectList(_context.Nationalities.OrderBy(o => o.Name), "Id", "Name", person.NationalityId);
            return View(person);
        }

        // POST: Person/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,SexId,Birthday,Obit,Nationality,Größe,Gewicht,PersonTypesId")] Person person)
        {
            if (id != person.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(person.Id))
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
            ViewData["PersonTypesId"] = new SelectList(_context.PersonType, "Id", "Name", person.PersonTypesId);
            ViewData["SexId"] = new SelectList(_context.Sex, "Id", "Name", person.SexId);
            return View(person);
        }

        // GET: Person/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .Include(p => p.PersonType)
                .Include(p => p.Sex)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: Person/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var person = await _context.Person.FindAsync(id);
            _context.Person.Remove(person);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(Guid id)
        {
            return _context.Person.Any(e => e.Id == id);
        }
    }
}