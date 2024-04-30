using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonApiController : ControllerBase
    {

        private readonly MvcMovieCoreContext _context;
        private List<MoviesPerson> _moviePersons;
        private List<Scenes> _scenes;

        public PersonApiController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        [HttpGet("{personId}")]
        public async Task<IActionResult> Get(Guid personId)
        {
            List<ViewModelPersonScene> personScenes = new List<ViewModelPersonScene>();
            var personMovies = await _context.MoviesPerson.Where(w => w.PersonId.Equals(personId)).ToListAsync();
            string jsonResult;
            var scenes = new List<int>();

            if (personMovies.Any())
            {
                foreach (var personMovie in personMovies)
                {
                    if (_scenes == null)
                        _scenes = await _context.Scenes.ToListAsync();

                    scenes = _scenes.Where(w => w.MoviesPersonsId.Equals(personMovie.Id)).Select(s => s.Scene).ToList();
                    if (scenes.Any())
                    {
                        foreach (var scene in scenes)
                        {
                            if (_moviePersons == null)
                                _moviePersons = await _context.MoviesPerson.Include(i => i.Movies).Include(i => i.Person).ToListAsync();

                            var moviePersons = _moviePersons.Where(w => w.MoviesId.Equals(personMovie.MoviesId) && !w.PersonId.Equals(personId));
                            foreach (var moviePerson in moviePersons)
                            {
                                var scenesCoActors = await _context.Scenes.Where(w => w.MoviesPersonsId.Equals(moviePerson.Id) && w.Scene == scene).ToListAsync();
                                foreach (var sceneCoActor in scenesCoActors)
                                {
                                    personScenes.Add(new ViewModelPersonScene()
                                    {
                                        Film = moviePerson.Movies.Name,
                                        Szene = sceneCoActor.Scene,
                                        Name = moviePerson.Person.Name,
                                    });
                                }
                            }
                        }
                    }
                }
                try
                {
                    jsonResult = JsonConvert.SerializeObject(personScenes.OrderBy(o => o.Name).ThenBy(t => t.Film).ThenBy(t => t.Szene), Formatting.Indented,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
                }
                catch (Exception ex)
                {
                    jsonResult = string.Empty;
                }
            }
            else
            {
                jsonResult = string.Empty;
            }
            return Ok(jsonResult);
        }
    }
}