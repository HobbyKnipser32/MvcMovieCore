using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.Controllers
{
    public class PersonController : Controller
    {
        private readonly MvcMovieCoreContext _context;
        private IHostingEnvironment _hostingEnvironment;

        public PersonController(MvcMovieCoreContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            var fileDic = "File";
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath , fileDic);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            var fileName = file.Name;
            filePath = Path.Combine(filePath, fileName);
            using (FileStream fs = System.IO.File.Create(filePath))
            {
                file.CopyTo(fs);
            }
            return Ok(file);
        }

        // GET: Person
        public IActionResult Index(string filter = null, string sortExpression = "Name", int page = 1)
        {
            var persons = _context.Person
                                  .Include(i => i.PersonType)
                                  .Include(i => i.Sex)
                                  .Include(i => i.Nationality)
                                  .OrderBy(o => o.Name)
                                  .AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter))
                persons = persons.Where(w => w.Name.Contains(filter));

            //var model = await PagingList.CreateAsync(persons, 20, page, sortExpression, "Name");

            return View(persons);
        }

        // GET: Person/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .Include(i => i.PersonType)
                .Include(i => i.Sex)
                .Include(i => i.Nationality)
                .Include(i => i.MoviesPerson)
                .ThenInclude(t => t.MovieRole)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            person.ActorsAge = person.GetActorsAge(person.Birthday, person.Obit);

            foreach (var mpId in person.MoviesPerson)
            {
                mpId.Movies = await _context.Movies.FirstOrDefaultAsync(f => f.Id == mpId.MoviesId);
                mpId.Movies.ActorsAge = mpId.Movies.GetActorsAgeInMovie(person.Birthday, mpId.Movies.YearOfPublication);
            }

            person.MoviesPerson = person.MoviesPerson.OrderBy(o => o.Movies.Name).ThenBy(t => t.Movies.YearOfPublication).ToList();

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
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,SexId,Birthday,Obit,NationalityId,Height,Weight,PersonTypesId,Classification")] Person person)
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
                .Include(i => i.PersonType)
                .Include(i => i.Sex)
                .Include(i => i.Nationality)
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
            if (person != null)
            {
                var moviePersons = await _context.MoviesPerson.Where(w => w.PersonId.Equals(person.Id)).ToListAsync();
                if (moviePersons.Any())
                {
                    foreach (var moviePerson in moviePersons)
                    {
                        var scenes = await _context.Scenes.Where(w => w.MoviesPersonsId.Equals(moviePerson.Id)).ToListAsync();
                        _context.Scenes.RemoveRange(scenes);
                    }
                    await _context.SaveChangesAsync();
                }
                _context.MoviesPerson.RemoveRange(moviePersons);
                await _context.SaveChangesAsync();
            }
            _context.Person.Remove(person);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Birthdays(string combine, DateTime birthday = new DateTime(), int year = 0)
        {
            if (birthday == DateTime.MinValue)
                birthday = DateTime.Today;

            var persons = new List<Person>();

            if (year > 0)
            {
                persons = await _context.Person
                                        .Where(w => w.Birthday.Value.Year == year)
                                        .Include(i => i.PersonType)
                                        .Include(i => i.Sex)
                                        .Include(i => i.Nationality)
                                        .Include(i => i.MoviesPerson)
                                        .OrderBy(o => o.PersonType)
                                        .ThenBy(t => t.Name).ToListAsync();
                ViewData["BirthDay"] = $" im Jahr {year}";
            }
            else
            {
                persons = await _context.Person
                                        .Where(w => w.Birthday.Value.Month == birthday.Month && w.Birthday.Value.Day == birthday.Day)
                                        .Include(i => i.PersonType)
                                        .Include(i => i.Sex)
                                        .Include(i => i.Nationality)
                                        .Include(i => i.MoviesPerson)
                                        .OrderBy(o => o.PersonType)
                                        .ThenBy(t => t.Name).ToListAsync();
                ViewData["BirthDay"] = $" {birthday.Day}.{birthday.Month:00}.";
            }

            if (combine != null && combine.ToLower().Equals("on"))
            {
                if (year == 0) year = birthday.Year;
                persons = await _context.Person
                                        .Where(w => w.Birthday.Value.Year == year && w.Birthday.Value.Month == birthday.Month && w.Birthday.Value.Day == birthday.Day)
                                        .Include(i => i.PersonType)
                                        .Include(i => i.Sex)
                                        .Include(i => i.Nationality)
                                        .Include(i => i.MoviesPerson)
                                        .OrderBy(o => o.PersonType)
                                        .ThenBy(t => t.Name).ToListAsync();
                ViewData["BirthDay"] = $" {birthday.Day:00}.{birthday.Month:00}.{year}";
            }

            foreach (var person in persons)
            {
                person.ActorsAge = person.Obit == null ? (birthday.Year - person.Birthday.Value.Year).ToString() : person.GetActorsAge(person.Birthday, person.Obit);
                foreach (var id in person.MoviesPerson)
                {
                    id.Movies = await _context.Movies.FirstOrDefaultAsync(f => f.Id == id.MoviesId);
                    id.Movies.ActorsAge = id.Movies.GetActorsAgeInMovie(person.Birthday, id.Movies.YearOfPublication);
                }
                person.MoviesPerson = person.MoviesPerson.OrderBy(o => o.Movies.YearOfPublication).ThenBy(t => t.Movies.Name).Take(5).ToList();
            }
            //ViewData["BirthDay"] = birthday.ToShortDateString();

            return View(persons);
        }

        private bool PersonExists(Guid id)
        {
            return _context.Person.Any(e => e.Id.Equals(id));
        }
    }
}