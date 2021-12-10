using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using System;
using System.Linq;

namespace MvcMoviesCore.Controllers
{
    public class GetRandomAdultMovieController : Controller
    {
        private readonly MvcMovieCoreContext _context;

        public GetRandomAdultMovieController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var movies = _context.Movies
                                 .Include(i => i.MoviesPerson)
                                 .ThenInclude(moviesPerson => moviesPerson.Person)
                                 .Where(w => w.Adult == true && w.InStock == true && !(w.OnWatch.Contains("o") || w.OnWatch.Contains("-")))
                                 .ToList();
            if (movies.Any())
            {
                int random = new Random().Next(1, movies.Count());
                var movie = movies[random];
                foreach (var moviePerson in movie.MoviesPerson)
                {
                    if (moviePerson.Person != null)
                    {
                        var personAge = new Person();
                        moviePerson.Person.ActorsAge = personAge.GetActorsMovieAge(moviePerson.Person.Birthday, movie.YearOfPublication);
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