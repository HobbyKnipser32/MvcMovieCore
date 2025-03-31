using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MvcMoviesCore.Models;
using MvcMoviesCore.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace MvcMoviesCore.Controllers
{
    public class MoviesController : Controller
    {
        #region private fields

        private readonly MvcMovieCoreContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _originalFileDirectory = "/Images/Original";
        private readonly bool _showAdult;

        #endregion

        #region constructor

        public MoviesController(MvcMovieCoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _showAdult = _configuration.GetValue<bool>("AppSettings:ShowAdult");
            _originalFileDirectory = _configuration.GetValue<string>("AppSettings:OriginalFileDirectory");
        }

        #endregion

        #region public functions

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            //var practices = GetPractices();

            ViewData["ShowAdult"] = _showAdult;
            await LoadDropdowns();
            return View();
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(i => i.Genre)
                .Include(i => i.RecordCarrier)
                .Include(i => i.StorageLocation)
                .Include(i => i.MoviesPerson)
                .ThenInclude(t => t.MovieRole)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }


            foreach (var mpId in movie.MoviesPerson)
            {
                mpId.Person = await _context.Person.Include(i => i.Sex).Include(i => i.PersonImages.Where(w => w.IsMain)).FirstOrDefaultAsync(f => f.Id == mpId.PersonId);
                if (mpId.Person.Obit != null && mpId.Person.Obit.Value.Year < movie.YearOfPublication.Value.Year)
                    mpId.Person.ActorsAge = mpId.Person.GetActorsAge(mpId.Person.Birthday, mpId.Person.Obit);
                else
                    mpId.Person.ActorsAge = mpId.Person.GetActorsAgeInMovie(mpId.Person.Birthday, movie.YearOfPublication);
            }

            movie.MoviesPerson = [.. movie.MoviesPerson.OrderBy(o => o.Person.Classification).ThenBy(t => t.Person.ActorsAge).ThenBy(t => t.Person.Name)];

            movie.Scenes = await GetScenesAsync(id);

            ViewData["ImageSource"] = _originalFileDirectory;
            return View(movie);
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
        public async Task<IActionResult> Create([Bind("Id,Name,YearOfPublication,GenreId,RecordCarrierId,InStock,StorageLocationId,Added,OnWatch,Remark,Adult,ThreeD,Owner,IMDB,Ranking,LastView")] Movies movie)
        {
            if (ModelState.IsValid)
            {
                movie.Id = Guid.NewGuid();
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenreId"] = new SelectList(_context.Genre.OrderBy(o => o.Name), "Id", "Name", movie.GenreId);
            ViewData["RecordCarrierId"] = new SelectList(_context.RecordCarrier.OrderBy(o => o.Name), "Id", "Name", movie.RecordCarrierId);
            ViewData["StorageLocationId"] = new SelectList(_context.StorageLocation.OrderBy(o => o.Name), "Id", "Name", movie.StorageLocationId);
            ViewData["Actors"] = GetActors();
            ViewData["Roles"] = GetRoles();
            return View(movie);
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
            ViewData["Actors"] = GetActors();
            ViewData["Roles"] = GetRoles();

            return View(movies);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,YearOfPublication,GenreId,RecordCarrierId,InStock,StorageLocationId,Added,OnWatch,Remark,Adult,ThreeD,Owner,IMDB,Ranking,LastView,ShortDescription")] Movies movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            //movies.Genre ??= await GetGenre(movies.GenreId);
            //ModelState.ClearValidationState(nameof(Movies));
            //TryValidateModel(movies);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MoviesExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return View("Details", movie);
            }
            ViewData["GenreId"] = new SelectList(_context.Genre.OrderBy(o => o.Name), "Id", "Name", movie.GenreId);
            ViewData["RecordCarrierId"] = new SelectList(_context.RecordCarrier.OrderBy(o => o.Name), "Id", "Name", movie.RecordCarrierId);
            ViewData["StorageLocationId"] = new SelectList(_context.StorageLocation.OrderBy(o => o.Name), "Id", "Name", movie.StorageLocationId);
            return View("Edit", movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> DeleteAsync(Guid? id)
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
        public async Task<IActionResult> DeleteConfirmedAsync(Guid id)
        {
            var movie = await _context.Movies.FindAsync(id);
            var moviesPerson = _context.MoviesPerson.Where(w => w.MoviesId == movie.Id).ToList();
            foreach (var moviePerson in moviesPerson)
            {
                var scenes = _context.Scenes.Where(w => w.MoviesPersonsId == moviePerson.Id).ToList();
                if (scenes.Count != 0)
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

        public async Task<IActionResult> DeleteLinkAsync(Guid? id)
        {
            if (id == null)
                return NotFound();

            var moviePerson = await _context.MoviesPerson
                                            .Include(i => i.Movies)
                                            .Include(i => i.Person)
                                            .FirstOrDefaultAsync(f => f.Id == id);
            if (moviePerson == null)
                return NotFound();

            ViewData["Referer"] = Request.Headers.Referer.ToString();

            return View(moviePerson);
        }

        [HttpPost, ActionName("DeleteLink")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLinkConfirmedAsync(Guid id)
        {
            var moviePerson = await _context.MoviesPerson.FindAsync(id);
            if (moviePerson != null)
            {
                var scenes = _context.Scenes.Where(w => w.MoviesPersonsId.Equals(moviePerson.Id)).ToList();
                if (scenes.Count != 0)
                {
                    _context.Scenes.RemoveRange(scenes);
                    await _context.SaveChangesAsync();
                }
                _context.MoviesPerson.Remove(moviePerson);
                await _context.SaveChangesAsync();
            }
            return Ok();
            //return RedirectToAction(nameof(Index));
        }

        public IActionResult FileSizes()
        {
            string moviesDirectory = _configuration.GetValue<string>("AppSettings:MoviesDirectory");
            string moviesAdultDirectory = _configuration.GetValue<string>("AppSettings:MoviesADirectory");
            //@"\\192.168.178.254\ErrorLogs\Errors\Filme"; // @"Y:\Errors\Filme";
            List<Movies> updateMovies = [];

            if (_showAdult)
            {
                updateMovies.AddRange(FileSizes(moviesAdultDirectory, true));
            }
            updateMovies.AddRange(FileSizes(moviesDirectory, false));

            return View(updateMovies);
        }

        #endregion

        #region private functions

        private async Task LoadDropdowns()
        {
            var genre = await _context.Genre.Where(w => !string.IsNullOrEmpty(w.Name)).OrderBy(o => o.Name).ToListAsync();
            ViewData["Genre"] = PrepareListWithNull([.. genre.Select(s => new SelectListItem(s.Name, s.Id.ToString()))]);
            var recordCarrier = await _context.RecordCarrier.Where(w => !string.IsNullOrEmpty(w.Name)).OrderBy(o => o.Name).ToListAsync();
            ViewData["RecordCarrier"] = PrepareListWithNull([.. recordCarrier.Select(s => new SelectListItem(s.Name, s.Id.ToString()))]);
        }

        private async Task<Genre> GetGenre(Guid? genreId)
        {
            if (genreId == null)
                return null;

            return await _context.Genre.FirstOrDefaultAsync(f => f.Id.Equals(genreId));

        }

        private List<Movies> FileSizes(string moviesPath, bool showAdult)
        {
            List<Movies> updateMovies = [];

            if (!Directory.Exists(moviesPath))
            {
                return updateMovies;
            }

            var movies = _context.Movies.Include(i => i.Genre).Where(w => w.Adult == showAdult).ToList();
            if (movies.Count != 0)
            {
                List<string> allowedFileExtensions = [".asf", ".avi", ".divx", ".flv", ".m4v", ".mkv", ".mov", ".mp4", ".mpeg", ".mpg", ".wmv"];
                var dirInfo = new DirectoryInfo(moviesPath);
                var filesInDirectory = dirInfo.GetFiles("*", SearchOption.AllDirectories);
                foreach (var file in filesInDirectory)
                {
                    if (allowedFileExtensions.Contains(file.Extension.ToLower()))
                    {
                        var fileName = file.Name[..^file.Extension.Length];
                        Movies movie = new();
                        if (showAdult)
                        {
                            movie = movies.FirstOrDefault(f => fileName.Contains(f.Name, StringComparison.CurrentCultureIgnoreCase)
                                && fileName.Contains(f.Genre?.Name, StringComparison.CurrentCultureIgnoreCase)
                                && (f.FileSize == null || f.FileSize == 0 || f.RunTime == null || f.RunTime == 0 || string.IsNullOrEmpty(f.FilePath)));
                        }
                        else
                        {
                            movie = movies.FirstOrDefault(f => fileName.Contains(f.Name, StringComparison.CurrentCultureIgnoreCase)
                                && file.DirectoryName.Contains(f.YearOfPublication.Value.Year.ToString())
                                && (f.FileSize == null || f.FileSize == 0 || f.RunTime == null || f.RunTime == 0 || string.IsNullOrEmpty(f.FilePath)));
                        }
                        if (movie != null)
                        {
                            movie.FileSize = file.Length;
                            movie.FilePath = file.DirectoryName;
                            movie.Added ??= file.LastWriteTime;
                            movie.RunTime ??= GetVideoDuration(file.FullName);
                            movie.ChangeDate = DateTime.Now;
                            movie.ChangeUser = Environment.UserName;
                            _context.Update(movie);
                            updateMovies.Add(movie);
                        }
                    }
                }
                _context.SaveChanges();
            }

            return updateMovies;
        }

        private List<SelectListItem> GetRoles()
        {
            return PrepareListWithNull([.. _context.MovieRole.Distinct().OrderBy(o => o.Name).Select(s => new SelectListItem(s.Name, s.Id.ToString()))]);
        }

        private List<SelectListItem> GetActors()
        {
            return PrepareListWithNull([.. _context.Person.Distinct().OrderBy(o => o.Name).Select(s => new SelectListItem(s.Name, s.Id.ToString()))]);
        }

        private bool MoviesExists(Guid id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        private async Task<List<ScenesViewModel>> GetScenesAsync(Guid? movieId)
        {
            var moviePersons = await _context.MoviesPerson
                .Where(w => w.MoviesId.Equals(movieId))
                .Include(i => i.Person)
                .ThenInclude(t => t.Sex)
                .ToListAsync();

            if (moviePersons.Count == 0)
                return null;

            var scenes = new List<ScenesViewModel>();

            foreach (var moviePerson in moviePersons)
            {
                var scenen = await _context.Scenes.Where(w => w.MoviesPersonsId.Equals(moviePerson.Id)).ToListAsync();
                if (scenen.Count != 0)
                {
                    foreach (var scene in scenen)
                    {
                        scenes.Add(new ScenesViewModel
                        {
                            Nr = scene.Scene,
                            Name = moviePerson.Person.Name,
                            Sex = moviePerson.Person.Sex.Name,
                            Classification = moviePerson.Person.Classification
                        });
                    }
                }
            }

            //var szenen = new List<string>();
            //foreach (var scene in scenes.GroupBy(g => g.Nr).OrderBy(o => o.Key))
            //{
            //    var x = scenes.Where(w => w.Nr == scene.Key).OrderBy(o => o.cl.Sex).ThenBy(o => o.Name).ToList();
            //    string szene = $"Szene {scene.Key}: ";
            //    foreach (var t in x)
            //    {
            //        szene += $"{t.Name}, ";
            //    }
            //    szene = szene.Substring(0, szene.Length - 2);
            //    szenen.Add(szene);
            //}

            return scenes;
        }

        private List<string> GetPractices()
        {
            var practices = new List<string>();
            var allPractices = _context.MoviesPerson.Where(w => !string.IsNullOrEmpty(w.Practices)).GroupBy(g => g.Practices).ToList();
            if (allPractices.Count != 0)
            {
                foreach (var practice in allPractices)
                {
                    if (practice.Key.Contains(','))
                    {
                        var keys = practice.Key.Split(',').ToList();
                        foreach (var key in keys)
                        {
                            if (!practices.Contains(key.Trim()))
                                practices.Add(key.Trim());
                        }
                    }
                    else
                    {
                        if (!practices.Contains(practice.Key.Trim()))
                            practices.Add(practice.Key.Trim());
                    }
                }
            }
            practices.Sort();
            return practices;
        }

        private List<string> GetFilterContent()
        {
            var filterContent = new List<string>();
            var genres = _context.Genre.Where(w => !string.IsNullOrEmpty(w.Name)).ToList();
            if (genres.Count != 0)
            {
                foreach (var genre in genres)
                {
                    var g = genre.Name.Split(".");
                    if (g.Length != 0)
                    {
                        foreach (var item in g)
                            if (!string.IsNullOrWhiteSpace(item) && !item.Contains('-') && !filterContent.Contains(item))
                                filterContent.Add(item.Trim());
                    }
                }
            }
            var practices = _context.MoviesPerson.Where(w => !string.IsNullOrEmpty(w.Practices));
            if (practices.Any())
            {
                foreach (var practic in practices)
                {
                    var p = practic.Practices.Split(",", StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length != 0)
                    {
                        foreach (var item in p)
                            if (!string.IsNullOrWhiteSpace(item) && !item.Contains('-') && !filterContent.Contains(item.Trim()))
                                filterContent.Add(item.Trim());
                    }
                }
            }
            return [.. filterContent.OrderBy(o => o)];
        }

        private static List<SelectListItem> PrepareListWithNull(List<SelectListItem> items)
        {
            items.Insert(0, new SelectListItem(null, null));
            return [.. items];
        }

        private static decimal GetVideoDuration(string filePath)
        {
            try
            {
                using var shell = ShellObject.FromParsingName(filePath);
                var t = TimeSpan.FromTicks((long)(ulong)((IShellProperty)shell.Properties.System.Media.Duration).ValueAsObject);
                var hour = t.Hours;
                var minute = t.Minutes;
                var second = decimal.Round((decimal)t.Seconds / 60, 2);

                decimal runTime = hour * 60 + minute + second;
                return runTime;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion
    }
}