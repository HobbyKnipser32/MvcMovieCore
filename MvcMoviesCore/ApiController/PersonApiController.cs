using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using MvcMoviesCore.Models;
using MvcMoviesCore.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonApiController : ControllerBase
    {
        #region private fields

        private readonly MvcMovieCoreContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private List<MoviesPerson> _moviePersons;
        private List<Scenes> _scenes;
        private readonly string _originalFilePath = @"images\Original";

        #endregion

        #region constructor

        public PersonApiController(MvcMovieCoreContext context, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _originalFilePath = _configuration.GetValue<string>("AppSettings:OriginalFilePath");
        }

        #endregion

        #region public functions

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

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromForm] Person person)
        {
            try
            {
                if (RenameImage(person, out var image))
                {
                    person.Image = image;
                }
                _context.Update(person);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (!string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    message = ex.InnerException.Message;
                }
                return BadRequest(message);
            }
        }

        #endregion

        #region private functions

        private bool RenameImage(Person person, out string newFileName)
        {
            newFileName = string.Empty;

            if (person == null)
                return false;

            if (string.IsNullOrEmpty(person.Image)) 
                return false; 

            var currentFileName = person.Image;
            var lastDot = currentFileName.LastIndexOf('.');
            var extension = currentFileName[(lastDot + 1)..];
            newFileName = $"{person.Id}.{extension}";
            var path = Path.Combine(_webHostEnvironment.WebRootPath, _originalFilePath);
            var sourceFile = Path.Combine(path, currentFileName);
            FileInfo fileInfo = new(sourceFile);
            if (fileInfo.Exists)
            {
                var targetFile = Path.Combine(path, newFileName);
                fileInfo.MoveTo(targetFile);
            }

            return true;
        }

        #endregion
    }
}