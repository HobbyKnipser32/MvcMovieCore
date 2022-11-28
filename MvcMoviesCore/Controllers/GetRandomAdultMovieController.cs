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
                int random = new Random().Next(0, movies.Count() - 1);
                var movie = movies[random];
                return Redirect($"Movies/Details/{movie.Id}");
                //foreach (var moviePerson in movie.MoviesPerson)
                //{
                //    if (moviePerson.Person != null)
                //    {
                //        var personAge = new Person();
                //        moviePerson.Person.ActorsAge = personAge.GetActorsMovieAge(moviePerson.Person.Birthday, movie.YearOfPublication);
                //        Sex sex = _context.Sex.FirstOrDefault(x => x.Id == moviePerson.Person.SexId);
                //        if (sex != null)
                //            moviePerson.Person.Sex.Name = sex.Name;
                //    }
                //}
                //movie.MoviesPerson = movie.MoviesPerson.OrderBy(o => o.Person.Sex.Name).ThenBy(t => t.Person.ActorsAge).ThenBy(t => t.Person.Name).ToList();
                //return RedirectToAction("Details", "Movies", movie.Id);
                    //View(movie);
            }

            return View();
        }
    }
}