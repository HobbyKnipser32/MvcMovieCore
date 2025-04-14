using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using MvcMoviesCore.ViewModels;
using Newtonsoft.Json;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        [HttpGet("GetPersonsWithoutImage")]
        public async Task<IActionResult> GetPersonsWithoutImage()
        {
            var persons = await _context.Person
                                  .Include(i => i.PersonType)
                                  .Include(i => i.Sex)
                                  .Include(i => i.Nationality)
                                  .Include(i => i.MoviesPerson)
                                  .Include(i => i.PersonImages)
                                  .Where(w => w.PersonImages.Count == 0)
                                  .OrderByDescending(o => o.MoviesPerson.Count)
                                  .ThenBy(o => o.Name)
                                  .ToListAsync();

            if (!_showAdult)
            {
                var personType = await _context.PersonType.FirstOrDefaultAsync(f => f.Name.ToLower().Contains("adult"));
                if (personType != null)
                    persons = persons.Where(w => !w.PersonTypesId.Equals(personType.Id)).ToList();
            }

            persons.ToList().ForEach(f => f.ActorsAge = f.GetActorsAge(f.Birthday, f.Obit));
            foreach (var person in persons)
            {
                person.MoviesPerson.ToList().ForEach(f => f.Person = null);
            }
            persons.ForEach(f => f.Nationality.Person = null);
            persons.ForEach(f => f.PersonType.Person = null);
            persons.ForEach(f => f.Sex.Person = null);

            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(persons.Where(w => w.MoviesPerson.Count > 0), Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var persons = await GetPersons();

            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(persons, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpGet("GetMovies/{personId}")]
        public async Task<IActionResult> GetMovies(Guid personId)
        {
            string jsonResult;
            var personMovies = await _context.MoviesPerson
                .Include(i => i.Person)
                .Include(i => i.Movies)
                .Include(i => i.MovieRole)
                .Where(w => w.PersonId.Equals(personId)).ToListAsync();
            List<PersonMoviesViewModel> movies = [];
            try
            {
                foreach (var personMovie in personMovies)
                {
                    var movie = new PersonMoviesViewModel()
                    {
                        Alter = personMovie.Movies.GetActorsAgeInMovie(personMovie.Person.Birthday, personMovie.Movies.YearOfPublication),
                        Bewertung = string.Format("{0:0.#}", personMovie.Movies.Ranking), //.ToString(),
                        Id = personMovie.MoviesId,
                        Laufzeit = string.Format("{0:0.0}", personMovie.Movies.RunTime), //.ToString(),
                        Name = personMovie.Movies.Name,
                        OnWatch = personMovie.Movies.OnWatch,
                        Role = personMovie.MovieRole?.Name != null ? personMovie.MovieRole.Name : string.Empty,
                        PersonMovieId = personMovie.Id
                    };
                    if (!string.IsNullOrEmpty(personMovie.Practices)) { movie.Praxis = personMovie.Practices; }
                    if (personMovie.Movies.YearOfPublication != null) { movie.Erscheinungsjahr = personMovie.Movies.YearOfPublication.Value.Year.ToString(); }
                    else { movie.Erscheinungsjahr = string.Empty; }
                    movies.Add(movie);
                }

                var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                jsonResult = JsonConvert.SerializeObject(movies.OrderBy(o => o.Name).ThenBy(t => t.Alter), Formatting.Indented, jsonSerializerSettings);
            }
            catch
            {
                jsonResult = string.Empty;
            }

            return Ok(jsonResult);
        }

        [HttpGet("GetScenes/{personId}")]
        public async Task<IActionResult> GetScenes(Guid personId)
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
                                        Film = moviePerson.Practices != null
                                            ? new Movies() { Id = moviePerson.Movies.Id, Name = $"{moviePerson.Movies.Name} ({moviePerson.Practices})" }
                                            : new Movies() { Id = moviePerson.Movies.Id, Name = moviePerson.Movies.Name },
                                        Szene = sceneCoActor.Scene.ToString(),
                                        Person = person,
                                    });
                                }
                            }
                        }

                        personScenes = [.. personScenes.OrderBy(o => o.Person).ThenBy(t => t.Film.Name).ThenBy(t => t.Szene)];
                    }
                    else
                    {
                        var actors = await _context
                            .MoviesPerson
                            .Include(i => i.Person)
                            .Include(i => i.Movies)
                            .Include(i => i.MovieRole)
                            .Where(w => w.MoviesId.Equals(personMovie.MoviesId) && !w.PersonId.Equals(personId))
                            .ToListAsync();
                        var referer = Request.GetTypedHeaders().Referer;
                        var hostUrl = $"{referer.Scheme}://{referer.Host}:{referer.Port}";
                        foreach (var actor in actors)
                        {
                            string person;
                            string url = $"{hostUrl}/Person/Details/{actor.PersonId}";
                            if (actor.Person.Classification < 8)
                            {
                                person = $"<b><a href=\"{url}\">{actor.Person.Name}</a></b>";
                            }
                            else
                            {
                                person = $"<a href=\"{url}\">{actor.Person.Name}</a>";
                            }
                            personScenes.Add(new PersonSceneViewModel()
                            {
                                Film = actor.MovieRole != null ? new Movies() { Id = personMovie.Movies.Id, Name = $"{personMovie.Movies.Name} ({actor.MovieRole.Name})" }
                                    : new Movies() { Id = personMovie.Movies.Id, Name = $"{personMovie.Movies.Name}" },
                                Person = person,
                                Szene = "-"
                            });
                        }

                        personScenes = [.. personScenes.OrderBy(o => o.Person).ThenBy(t => t.Film.Name)];
                    }
                }
                try
                {
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
                jsonResult = JsonConvert.SerializeObject(new List<PersonSceneViewModel>());
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
                                            .Include(i => i.PersonImages.Where(f => f.IsMain == true))
                                            .OrderBy(o => o.PersonType)
                                            .ThenBy(t => t.Name).ToListAsync();

                foreach (var person in persons)
                {
                    var actorsAge = (today.Year - person.Birthday.Value.Year).ToString();
                    if (person.Obit != null)
                        actorsAge += $" ({person.GetActorsAge(person.Birthday, person.Obit)})";
                    person.ActorsAge = actorsAge;
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
                                            .Include(i => i.PersonImages.Where(f => f.IsMain == true))
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
        public async Task<IActionResult> Update([FromForm] PersonViewModel person)
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

        [HttpGet("GetImages/{personId}")]
        public async Task<IActionResult> GetImages(Guid personId)
        {
            if (personId == Guid.Empty)
                return BadRequest();

            var images = await _context.PersonImage.Where(w => w.PersonId.Equals(personId) && w.IsDeleted == false).ToListAsync();

            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(images, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("SaveImage")]
        public async Task<IActionResult> SaveImage(Guid personId)
        {
            if (personId.Equals(Guid.Empty))
                return BadRequest();

            int imageNumber = 0;
            try
            {
                imageNumber = await _context.PersonImage.Where(w => w.PersonId.Equals(personId)).MaxAsync(m => m.Number);
            }
            catch
            {
                imageNumber = 0;
            }
            imageNumber++;
            var file = Request.Form.Files[0];
            var fileExtension = Path.GetExtension(file.FileName);
            var newFileName = $"{imageNumber}{fileExtension}";
            var newPath = Path.Combine(_webHostEnvironment.WebRootPath, _originalFilePath, personId.ToString());
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            var newFilePath = Path.Combine(newPath, newFileName);
            try
            {

                Stream fileStream = file.OpenReadStream();
                fileStream.CopyToFile(newFilePath);
                bool isMain = false;
                if (imageNumber == 1)
                    isMain = true;
                var personImage = new PersonImage()
                {
                    ChangedAt = DateTime.Now,
                    CreatetAt = DateTime.Now,
                    Id = Guid.NewGuid(),
                    IsDeleted = false,
                    IsMain = isMain,
                    Name = newFileName,
                    Number = imageNumber,
                    PersonId = personId
                };
                await _context.AddAsync(personImage);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteImage")]
        public async Task<IActionResult> DeleteImage(Guid id, Guid personId)
        {
            try
            {
                var personImage = await _context.PersonImage.FirstOrDefaultAsync(f => f.Id.Equals(id));
                if (personImage != null)
                {
                    personImage.IsDeleted = true;
                    if (personImage.IsMain)
                    {
                        personImage.IsMain = false;
                        var personImageNumber = await _context.PersonImage.Where(f => f.PersonId.Equals(personId) && f.IsDeleted == false).MinAsync(m => m.Number);
                        if (personImageNumber != 0)
                        {
                            var personImageMain = await _context.PersonImage.FirstOrDefaultAsync(f => f.PersonId.Equals(personId) && f.Number == personImageNumber);
                            personImageMain.IsMain = true;
                            _context.Update(personImageMain);
                        }
                    }
                    _context.Update(personImage);
                    await _context.SaveChangesAsync();

                }
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, _originalFilePath, personImage.PersonId.ToString(), personImage.Name);
                System.IO.File.Delete(filePath);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Delete/{personId}")]
        public async Task<IActionResult> Delete(Guid personId)
        {
            if (personId == Guid.Empty)
                return BadRequest();

            var person = await _context.Person.FirstOrDefaultAsync(f => f.Id.Equals(personId));

            if (person == null)
                return BadRequest(HttpStatusCode.NotFound);

            var personImages = await _context.PersonImage.Where(w => w.PersonId.Equals(personId)).ToListAsync();
            if (personImages.Count != 0)
                _context.PersonImage.RemoveRange(personImages);

            var personMovies = await _context.MoviesPerson.Where(w => w.PersonId.Equals(personId)).ToListAsync();
            if (personMovies.Count != 0)
                _context.MoviesPerson.RemoveRange(personMovies);

            _context.Person.Remove(person);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("Filter")]
        public async Task<IActionResult> Filter([FromForm] FilterPersonViewModel filter)
        {
            var persons = await GetPersons();
            decimal heightFrom = 0;
            decimal heightTo = 0;
            decimal weightFrom = 0;
            decimal weightTo = 0;

            if (!string.IsNullOrEmpty(filter.HeightFrom))
                heightFrom = decimal.Parse(filter.HeightFrom) / 100;
            if (!string.IsNullOrEmpty(filter.HeightTo))
                heightTo = decimal.Parse(filter.HeightTo) / 100;
            if (!string.IsNullOrEmpty(filter.WeightFrom))
                weightFrom = decimal.Parse(filter.WeightFrom);
            if (!string.IsNullOrEmpty(filter.WeightTo))
                weightTo = decimal.Parse(filter.WeightTo);

            if (!string.IsNullOrEmpty(filter.Name))
                persons = [.. persons.Where(w => w.Name.ToLower().Contains(filter.Name.ToLower()))];
            if (filter.YearOfBirth != null && filter.YearOfBirth > 0)
                persons = [.. persons.Where(w => w.Birthday.GetValueOrDefault().Year == filter.YearOfBirth)];
            if (filter.Nationality != null && filter.Nationality != Guid.Empty)
                persons = [.. persons.Where(w => w.NationalityId.Equals(filter.Nationality))];
            if (heightFrom > 0 && heightTo == 0)
                persons = [.. persons.Where(w => w.Height >= heightFrom)];
            else if (heightFrom == 0 && heightTo > 0)
                persons = [.. persons.Where(w => w.Height <= heightTo)];
            else if (heightFrom > 0 && heightTo > 0)
                persons = [.. persons.Where(w => w.Height >= heightFrom && w.Height <= heightTo)];
            if (weightFrom > 0 && weightTo == 0)
                persons = [.. persons.Where(w => w.Weight >= weightFrom)];
            else if (weightFrom == 0 && weightTo > 0)
                persons = [.. persons.Where(w => w.Weight <= weightTo)];
            else if (weightFrom > 0 && weightTo > 0)
                persons = [.. persons.Where(w => w.Weight >= weightFrom && w.Weight <= weightTo)];
            if (filter.PersonType != null && filter.PersonType != Guid.Empty)
                persons = [.. persons.Where(w => w.PersonTypesId.Equals(filter.PersonType))];
            if (filter.Sex != null && filter.Sex != Guid.Empty)
                persons = [.. persons.Where(w => w.SexId.Equals(filter.Sex))];

            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(persons, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("DeleteMoviePerson")]
        public async Task<IActionResult> DeleteMoviePerson(Guid moviePersonId)
        {
            try
            {
                var moviesPerson = await _context.MoviesPerson.Where(w => w.Id.Equals(moviePersonId)).ToListAsync();
                foreach (var moviePersonItem in moviesPerson)
                {
                    var scenes = await _context.Scenes.Where(w => w.MoviesPersonsId == moviePersonItem.Id).ToListAsync();
                    if (scenes.Count != 0)
                    {
                        _context.Scenes.RemoveRange(scenes);
                        await _context.SaveChangesAsync();
                    }
                    _context.MoviesPerson.Remove(moviePersonItem);
                    await _context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        #endregion

        #region private functions

        private async Task<List<Person>> GetPersons()
        {
            var persons = await _context.Person
                                  .Include(i => i.PersonType)
                                  .Include(i => i.Sex)
                                  .Include(i => i.Nationality)
                                  .Include(i => i.MoviesPerson)
                                  .Include(i => i.PersonImages.Where(w => w.IsMain == true && w.IsDeleted == false))
                                  .OrderBy(o => o.Name)
                                  .ToListAsync();

            if (!_showAdult)
            {
                var personType = _context.PersonType.FirstOrDefault(f => f.Name.ToLower().Contains("adult"));
                if (personType != null)
                    persons = persons.Where(w => !w.PersonTypesId.Equals(personType.Id)).ToList();
            }

            persons.ToList().ForEach(f => f.ActorsAge = f.GetActorsAge(f.Birthday, f.Obit));
            foreach (var person in persons)
            {
                person.MoviesPerson.ToList().ForEach(f => f.Person = null);
            }
            persons.ForEach(f => f.Nationality.Person = null);
            persons.ForEach(f => f.PersonType.Person = null);
            persons.ForEach(f => f.Sex.Person = null);

            return persons;
        }

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
                fileInfo.MoveTo(targetFile, true);
            }

            return true;
        }

        #endregion
    }
}