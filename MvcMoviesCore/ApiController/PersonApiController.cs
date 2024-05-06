using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using MvcMoviesCore.Models;
using MvcMoviesCore.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

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
            List<PersonSceneViewModel> personScenes = new List<PersonSceneViewModel>();
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
                                    string person;
                                    var referer = Request.GetTypedHeaders().Referer;
                                    var hostUrl = $"{referer.Scheme}://{referer.Host}:{referer.Port}";
                                    string url = $"{hostUrl}/Person/Details/{moviePerson.Person.Id}";
                                    if (moviePerson.Person.Classification < 8)
                                    {
                                        person = $"<b><a href=\"{url}\">{moviePerson.Person.Name}</a></b>";
                                    }
                                    else
                                    {
                                        person = $"<a href=\"{url}\">{moviePerson.Person.Name}</a>";
                                    }
                                    personScenes.Add(new PersonSceneViewModel()
                                    {
                                        Film = new Movies() { Id = moviePerson.Movies.Id, Name = moviePerson.Movies.Name },
                                        Szene = sceneCoActor.Scene,
                                        Person = person,
                                        //Person = new Person() { Id = moviePerson.Person.Id, Name = moviePerson.Person.Name }
                                    });
                                }
                            }
                        }
                    }
                }
                try
                {
                    personScenes = personScenes.OrderBy(o => o.Person).ThenBy(t => t.Film.Name).ThenBy(t => t.Szene).ToList();
                    var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                    jsonResult = JsonConvert.SerializeObject(personScenes, Formatting.Indented, jsonSerializerSettings);
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