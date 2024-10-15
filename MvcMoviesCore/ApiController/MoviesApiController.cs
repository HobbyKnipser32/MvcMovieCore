using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using MvcMoviesCore.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesApiController(MvcMovieCoreContext context, IConfiguration configuration) : ControllerBase
    {

        private readonly MvcMovieCoreContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet("{movieId}")]
        public async Task<IActionResult> Get(Guid movieId)
        {
            var movie = await _context.Movies.FirstOrDefaultAsync(f => f.Id.Equals(movieId));
            var moviePersons = await _context.MoviesPerson.Include(i => i.MovieRole).Where(w => w.MoviesId.Equals(movieId)).ToListAsync();
            foreach (var moviePerson in moviePersons)
            {
                moviePerson.Person = await _context
                    .Person
                    .Include(i => i.Sex)
                    .Include(i => i.PersonImages.Where(w => w.IsMain == true))
                    .FirstOrDefaultAsync(f => f.Id == moviePerson.PersonId);
                if (moviePerson.Person.Obit != null && moviePerson.Person.Obit.Value.Year < movie.YearOfPublication.Value.Year)
                    moviePerson.Person.ActorsAge = moviePerson.Person.GetActorsAge(moviePerson.Person.Birthday, moviePerson.Person.Obit);
                else
                    moviePerson.Person.ActorsAge = moviePerson.Person.GetActorsAgeInMovie(moviePerson.Person.Birthday, movie.YearOfPublication);
            }

            moviePersons = moviePersons.OrderBy(o => o.Person.Classification).ThenBy(t => t.Person.ActorsAge).ThenBy(t => t.Person.Name).ToList();
            string jsonResult;
            try
            {
                jsonResult = JsonConvert.SerializeObject(moviePersons, Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
            }
            catch (Exception)
            {
                jsonResult = string.Empty;
            }
            //return Ok();
            return Ok(jsonResult);
        }

        [HttpGet("GetActors/{movieId}")]
        public async Task<IActionResult> GetActors(Guid movieId)
        {
            string jsonResult;

            if (movieId == Guid.Empty)
                return Ok(string.Empty);

            var moviePersons = new List<MoviePersonsViewModel>();
            var movieRoles = await _context.MoviesPerson
                .Include(i => i.Person)
                .Include(i => i.MovieRole)
                .Where(w => w.MoviesId.Equals(movieId))
                .ToListAsync();
            foreach (var movieRole in movieRoles)
            {
                var viewModel = new MoviePersonsViewModel() { Actor = movieRole.Person.Name };
                if (movieRole.MovieRole != null)
                {
                    viewModel.Role = movieRole.MovieRole.Name;
                    viewModel.MovieRoleId = movieRole.MovieRole.Id;
                }
                moviePersons.Add(viewModel);
            }
            try
            {
                moviePersons = moviePersons.OrderBy(o => o.Actor).ThenBy(t => t.Role).ToList();
                var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                jsonResult = JsonConvert.SerializeObject(moviePersons, Formatting.Indented, jsonSerializerSettings);
                return Ok(jsonResult);
            }
            catch (Exception)
            {
                return Ok(string.Empty);
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var moviePerson = await _context.MoviesPerson.FirstOrDefaultAsync(f => f.Id.Equals(id));
            var scenes = await _context.Scenes.Where(w => w.MoviesPersonsId.Equals(moviePerson.Id)).ToListAsync();
            if (scenes.Count != 0)
            {
                _context.Scenes.RemoveRange(scenes);
                _context.SaveChanges();
            }
            _context.MoviesPerson.Remove(moviePerson);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("GetMovies")]
        public async Task<IActionResult> GetMovies()
        {
            string jsonResult;
            var showAdult = _configuration.GetValue<bool>("AppSettings:ShowAdult");
            var movies = await _context.Movies
                                .Include(m => m.Genre)
                                .Include(m => m.RecordCarrier)
                                .Include(m => m.StorageLocation)
                                .OrderBy(o => o.Name).ToListAsync();
            //.AsQueryable();

            foreach (var movie in movies)
            {
                movie.Genre.Movies.Clear();
                movie.RecordCarrier?.Movies.Clear();
                movie.StorageLocation?.Movies.Clear();
            }

            if (!showAdult)
                movies = movies.Where(w => w.Adult == false).ToList();

            try
            {
                var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                jsonResult = JsonConvert.SerializeObject(movies, Formatting.Indented, jsonSerializerSettings);
                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPost("DeleteMovie/{id}")]
        public async Task<IActionResult> DeleteMovie(Guid id)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(id);
                var moviesPerson = await _context.MoviesPerson.Where(w => w.MoviesId == movie.Id).ToListAsync();
                foreach (var moviePerson in moviesPerson)
                {
                    var scenes = await _context.Scenes.Where(w => w.MoviesPersonsId == moviePerson.Id).ToListAsync();
                    if (scenes.Any())
                    {
                        _context.Scenes.RemoveRange(scenes);
                        await _context.SaveChangesAsync();
                    }
                    _context.MoviesPerson.Remove(moviePerson);
                    await _context.SaveChangesAsync();
                }

                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
    }
}