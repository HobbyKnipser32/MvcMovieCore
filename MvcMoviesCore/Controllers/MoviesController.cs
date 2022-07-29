using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using ReflectionIT.Mvc.Paging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MvcMovieCoreContext _context;

        private bool _showAdult;

        public MoviesController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string filter, string sortExpression = "Name", int page = 1, bool adult = true)
        {
            _showAdult = adult;

            var mvcMovieCoreContext = _context.Movies
                                              .Include(m => m.Genre)
                                              .Include(m => m.RecordCarrier)
                                              .Include(m => m.StorageLocation)
                                              .OrderBy(o => o.Name)
                                              .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
                mvcMovieCoreContext = mvcMovieCoreContext.Where(p => p.Name.Contains(filter));

            if (!_showAdult)
                mvcMovieCoreContext = mvcMovieCoreContext.Where(w => w.Adult == false);

            var model = await PagingList.CreateAsync(mvcMovieCoreContext, 10, page, sortExpression, "Name");

            model.RouteValue = new RouteValueDictionary { { "filter", filter } };

            return View(model);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movies = await _context.Movies
                .Include(i => i.Genre)
                .Include(i => i.RecordCarrier)
                .Include(i => i.StorageLocation)
                .Include(i => i.MoviesPerson)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movies == null)
            {
                return NotFound();
            }

            foreach (var mpId in movies.MoviesPerson)
            {
                mpId.Person = await _context.Person.Include(i => i.Sex).FirstOrDefaultAsync(f => f.Id == mpId.PersonId);
                mpId.Person.ActorsAge = mpId.Person.GetActorsMovieAge(mpId.Person.Birthday, movies.YearOfPublication);
            }

            movies.MoviesPerson = movies.MoviesPerson.OrderBy(o => o.Person.Sex.Name).ThenBy(t => t.Person.ActorsAge).ThenBy(t => t.Person.Name).ToList();

            return View(movies);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Genre.OrderBy(o => o.Name), "Id", "Name");
            ViewData["RecordCarrierId"] = new SelectList(_context.RecordCarrier.OrderBy(o => o.Name), "Id", "Name");
            ViewData["StorageLocationId"] = new SelectList(_context.StorageLocation.OrderBy(o => o.Name), "Id", "Name");
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,YearOfPublication,GenreId,RecordCarrierId,InStock,StorageLocationId,Added,OnWatch,Remark,Adult,ThreeD,Owner,IMDB,Ranking,LastView")] Movies movies)
        {
            if (ModelState.IsValid)
            {
                movies.Id = Guid.NewGuid();
                _context.Add(movies);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenreId"] = new SelectList(_context.Genre.OrderBy(o => o.Name), "Id", "Name", movies.GenreId);
            ViewData["RecordCarrierId"] = new SelectList(_context.RecordCarrier.OrderBy(o => o.Name), "Id", "Name", movies.RecordCarrierId);
            ViewData["StorageLocationId"] = new SelectList(_context.StorageLocation.OrderBy(o => o.Name), "Id", "Name", movies.StorageLocationId);
            return View(movies);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movies = await _context.Movies.FindAsync(id);
            if (movies == null)
            {
                return NotFound();
            }
            ViewData["GenreId"] = new SelectList(_context.Genre.OrderBy(o => o.Name), "Id", "Name", movies.GenreId);
            ViewData["RecordCarrierId"] = new SelectList(_context.RecordCarrier.OrderBy(o => o.Name), "Id", "Name", movies.RecordCarrierId);
            ViewData["StorageLocationId"] = new SelectList(_context.StorageLocation.OrderBy(o => o.Name), "Id", "Name", movies.StorageLocationId);
            return View(movies);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,YearOfPublication,GenreId,RecordCarrierId,InStock,StorageLocationId,Added,OnWatch,Remark,Adult,ThreeD,Owner,IMDB,Ranking,LastView")] Movies movies)
        {
            if (id != movies.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movies);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MoviesExists(movies.Id))
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
            ViewData["GenreId"] = new SelectList(_context.Genre.OrderBy(o => o.Name), "Id", "Name", movies.GenreId);
            ViewData["RecordCarrierId"] = new SelectList(_context.RecordCarrier.OrderBy(o => o.Name), "Id", "Name", movies.RecordCarrierId);
            ViewData["StorageLocationId"] = new SelectList(_context.StorageLocation.OrderBy(o => o.Name), "Id", "Name", movies.StorageLocationId);
            return View(movies);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movies = await _context.Movies
                .Include(m => m.Genre)
                .Include(m => m.RecordCarrier)
                .Include(m => m.StorageLocation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movies == null)
            {
                return NotFound();
            }

            return View(movies);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var movie = await _context.Movies.FindAsync(id);
            var moviesPerson = _context.MoviesPerson.Where(w => w.MoviesId == movie.Id).ToList();
            foreach (var moviePerson in moviesPerson)
            {
                var scenes = _context.Scenes.Where(w => w.MoviesPersonsId == moviePerson.Id).ToList();
                if (scenes.Any())
                {
                    _context.Scenes.RemoveRange(scenes);
                    _context.SaveChanges();
                }
                _context.MoviesPerson.Remove(moviePerson);
                _context.SaveChanges();
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteLink(Guid? id)
        {
            if (id == null)
                return NotFound();

            var moviePerson = await _context.MoviesPerson
                                            .Include(i => i.Movies)
                                            .Include(i => i.Person)
                                            .FirstOrDefaultAsync(f => f.Id == id);
            if (moviePerson == null)
                return NotFound();

            return View(moviePerson);
        }

        [HttpPost, ActionName("DeleteLink")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLinkConfirmed(Guid? id)
        {
            var moviePerson = await _context.MoviesPerson.FindAsync(id);
            if (moviePerson != null)
            {
                _context.MoviesPerson.Remove(moviePerson);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MoviesExists(Guid id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}