using Microsoft.AspNetCore.Mvc;
using MvcMoviesCore.Models;
using System;
using System.Linq;

namespace MvcMoviesCore.Controllers
{
    public class GetRandomAdultMovieController : Controller
    {
        MvcMovieCoreContext _context;

        public GetRandomAdultMovieController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var movies = _context.Movies.Where(w => w.Adult == true && (w.OnWatch != "o" || w.OnWatch != "-")).ToList();
            if (movies.Any())
            {
                int random = new Random().Next(1, movies.Count());
                var movie = movies[random];
                var moviePerson = _context.MoviesPerson.Where(w => w.MoviesId == movie.Id);
                foreach (var mp in moviePerson)
                {
                    Person person = _context.Person.Where(w => w.Id == mp.PersonId).FirstOrDefault();
                    if (person != null)
                    {
                        person.ActorsAge = person.GetActorsMovieAge(person.Birthday, movie.YearOfPublication);
                        Sex sex = _context.Sex.Where(w => w.Id == person.SexId).FirstOrDefault();
                        if (sex != null)
                            person.Sex.Name = sex.Name;
                        mp.Person = person;
                    }
                    movie.MoviesPerson.Add(mp);
                }
                movie.MoviesPerson = movie.MoviesPerson.OrderBy(o => o.Person.Sex.Name).ThenBy(t => t.Person.ActorsAge).ThenBy(t => t.Person.Name).ToList();
                return View(movie);
            }

            return View();
        }
    }
}