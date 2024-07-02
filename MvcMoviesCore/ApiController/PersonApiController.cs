using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly bool _showAdult;

        #endregion

        #region constructor

        public PersonApiController(MvcMovieCoreContext context, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _originalFilePath = _configuration.GetValue<string>("AppSettings:OriginalFilePath");
            _showAdult = _configuration.GetValue<bool>("AppSettings:ShowAdult");
        }

        #endregion

        #region public functions

        [HttpGet("{personId}")]
        public async Task<IActionResult> Get(Guid personId)
        {
            List<PersonSceneViewModel> personScenes = [];
            var personMovies = await _context.MoviesPerson.Where(w => w.PersonId.Equals(personId)).ToListAsync();
            string jsonResult;
            var scenes = new List<int>();

            if (personMovies.Count != 0)
            {
                foreach (var personMovie in personMovies)
                {
                    _scenes ??= await _context.Scenes.ToListAsync();

                    scenes = _scenes.Where(w => w.MoviesPersonsId.Equals(personMovie.Id)).Select(s => s.Scene).ToList();
                    if (scenes.Count != 0)
                    {
                        foreach (var scene in scenes)
                        {
                            _moviePersons ??= await _context.MoviesPerson.Include(i => i.Movies).Include(i => i.Person).ToListAsync();

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
                catch (Exception)
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

        [HttpGet("TodaysBirtdays")]
        public async Task<IActionResult> TodaysBirtdays()
        {
            try
            {
                var today = DateTime.Now;
                var persons = await _context.Person
                                            .Where(w => w.Birthday.Value.Month == today.Month && w.Birthday.Value.Day == today.Day)
                                            .Include(i => i.PersonType)
                                            .Include(i => i.Sex)
                                            .Include(i => i.Nationality)
                                            .OrderBy(o => o.PersonType)
                                            .ThenBy(t => t.Name).ToListAsync();

                foreach (var person in persons)
                {
                    person.ActorsAge = person.Obit == null ? (today.Year - person.Birthday.Value.Year).ToString() : person.GetActorsAge(person.Birthday, person.Obit);
                    if (person.Nationality.Person != null) { person.Nationality.Person = null; }
                    if (person.PersonType.Person != null) { person.PersonType.Person = null; }
                    if (person.Sex.Person != null) { person.Sex.Person = null; }
                }

                if (!_showAdult)
                    persons = persons.Where(w => !w.PersonType.Name.Contains("adult", StringComparison.CurrentCultureIgnoreCase)).ToList();

                var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                var jsonResult = JsonConvert.SerializeObject(persons, Formatting.Indented, jsonSerializerSettings);

                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message = ex.InnerException.Message;
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("TodaysObits")]
        public async Task<IActionResult> TodaysObits()
        {
            try
            {
                var today = DateTime.Now;
                var persons = await _context.Person
                                            .Where(w => w.Obit.Value.Month == today.Month && w.Obit.Value.Day == today.Day)
                                            .Include(i => i.PersonType)
                                            .Include(i => i.Sex)
                                            .Include(i => i.Nationality)
                                            .OrderBy(o => o.PersonType)
                                            .ThenBy(t => t.Name).ToListAsync();

                foreach (var person in persons)
                {
                    person.ActorsAge = person.Obit == null ? (today.Year - person.Birthday.Value.Year).ToString() : person.GetActorsAge(person.Birthday, person.Obit);
                    if (person.Nationality.Person != null) { person.Nationality.Person = null; }
                    if (person.PersonType.Person != null) { person.PersonType.Person = null; }
                    if (person.Sex.Person != null) { person.Sex.Person = null; }
                }

                if (!_showAdult)
                    persons = persons.Where(w => !w.PersonType.Name.Contains("adult", StringComparison.CurrentCultureIgnoreCase)).ToList();

                var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                var jsonResult = JsonConvert.SerializeObject(persons, Formatting.Indented, jsonSerializerSettings);

                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message = ex.InnerException.Message;
                return BadRequest(ex.Message);
            }
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
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    message = ex.InnerException.Message;
                }
                return BadRequest(message);
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] Person person)
        {
            try
            {
                if (string.IsNullOrEmpty(person.Name))
                    return BadRequest("Der Name ist erforderlich!");

                person.Id = Guid.NewGuid();
                if (RenameImage(person, out var image))
                {
                    person.Image = image;
                }

                _context.Add(person);
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