﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesApiController : ControllerBase
    {

        private readonly MvcMovieCoreContext _context;

        public MoviesApiController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        [HttpGet("{movieId}")]
        public async Task<IActionResult> Get(Guid movieId)
        {
            var movie = await _context.Movies.FirstOrDefaultAsync(f => f.Id.Equals(movieId));
            var moviePersons = await _context.MoviesPerson.Where(w => w.MoviesId.Equals(movieId)).ToListAsync();
            foreach (var moviePerson in moviePersons)
            {
                moviePerson.Person = await _context.Person.Include(i => i.Sex).FirstOrDefaultAsync(f => f.Id == moviePerson.PersonId);
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
            catch (Exception ex)
            {
                jsonResult = string.Empty;
            }
            //return Ok();
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var moviePerson = await _context.MoviesPerson.FirstOrDefaultAsync(f => f.Id.Equals(id));
            var scenes = await _context.Scenes.Where(w => w.MoviesPersonsId.Equals(moviePerson.Id)).ToListAsync();
            if (scenes.Any())
            {
                _context.Scenes.RemoveRange(scenes);
                _context.SaveChanges();
            }
            _context.MoviesPerson.Remove(moviePerson);
            _context.SaveChanges();

            return Ok();
        }
    }
}