using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using System;
using System.Linq;

namespace MvcMoviesCore.Controllers
{
    public class GetRandomMovieController : Controller
    {
        private readonly MvcMovieCoreContext _context;

        public GetRandomMovieController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var movies = _context.Movies
                                 .Include(i => i.RecordCarrier)
                                 .Include(i => i.MoviesPerson)
                                 .ThenInclude(moviesPerson => moviesPerson.Person)
                                 .Where(w => w.Adult == false && w.LastView == null && w.InStock == true)
                                 .ToList();
            if (movies.Any())
            {
                int random = new Random().Next(1, movies.Count());
                var movie = movies[random];
                foreach(var moviePerson in movie.MoviesPerson)
                {
                    if (moviePerson.Person != null)
                    {
                        moviePerson.Person.ActorsAge = moviePerson.Person.GetActorsMovieAge(moviePerson.Person.Birthday, movie.YearOfPublication);
                        Sex sex = _context.Sex.Where(w => w.Id == moviePerson.Person.SexId).FirstOrDefault();
                        if (sex != null)
                            moviePerson.Person.Sex.Name = sex.Name;
                    }
                }
                movie.MoviesPerson = movie.MoviesPerson.OrderBy(o => o.Person.Sex.Name).ThenBy(t => t.Person.ActorsAge).ThenBy(t => t.Person.Name).ToList();
                return View(movie);
            }

            return View();
        }
    }
}