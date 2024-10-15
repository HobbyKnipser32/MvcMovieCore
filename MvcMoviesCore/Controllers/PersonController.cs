using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using MvcMoviesCore.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.Controllers
{
    public class PersonController : Controller
    {
        #region fields

        private readonly MvcMovieCoreContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly bool _showAdult = false;
        private readonly string _originalFileDirectory = "/Images/Original";
        private readonly string _originalFilePath = @"images\Original";

        #endregion

        #region constructor

        public PersonController(MvcMovieCoreContext context, IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _webHostEnvironment = hostEnvironment;
            _showAdult = _configuration.GetValue<bool>("AppSettings:ShowAdult");
            _originalFileDirectory = _configuration.GetValue<string>("AppSettings:OriginalFileDirectory");
            _originalFilePath = _configuration.GetValue<string>("AppSettings:OriginalFilePath");
        }

        #endregion

        #region puclic functions

        public async Task<IActionResult> UpdatedImages()
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, _originalFilePath);
            List<Person> persons = [];

            if (Directory.Exists(filePath))
            {
                var files = Directory.GetFiles(filePath).ToList();
                foreach (var file in files)
                {
                    var lastBackSlash = file.LastIndexOf('\\');
                    var fileNameWithExtension = file[(lastBackSlash + 1)..];
                    var lastDot = fileNameWithExtension.LastIndexOf('.');
                    var fileName = fileNameWithExtension[..lastDot];
                    Guid personId;
                    try
                    {
                        personId = new Guid(fileName);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    var person = _context.Person.FirstOrDefaultAsync(x => x.Id == personId).GetAwaiter().GetResult();
                    if (person != null && string.IsNullOrEmpty(person.Image))
                    {
                        person.Image = fileNameWithExtension;
                        _context.Update(person);
                        await _context.SaveChangesAsync();
                        persons.Add(person);
                    }
                }
            }
            ViewData["ImagesSource"] = _originalFileDirectory;
            return View(persons);
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, _originalFilePath);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            var fileName = file.FileName;
            filePath = Path.Combine(filePath, fileName);
            using (FileStream fs = System.IO.File.Create(filePath))
            {
                file.CopyTo(fs);
            }
            return Ok(file);
        }

        // GET: Person
        public IActionResult Index()
        {

            ViewData["OriginalFileDirectory"] = _originalFileDirectory;
            return View();
        }

        public IActionResult FilterNationality(Guid id)
        {
            if (id == Guid.Empty)
                return Redirect("Nationality");

            var persons = _context.Person
                                  .Include(i => i.PersonType)
                                  .Include(i => i.Sex)
                                  .Include(i => i.Nationality)
                                  .Where(w => w.NationalityId.Equals(id))
                                  .OrderBy(o => o.Name)
                                  .AsQueryable();

            if (!_showAdult)
            {
                var personType = _context.PersonType.FirstOrDefault(f => f.Name.ToLower().Contains("adult"));
                if (personType != null)
                    persons = persons.Where(w => !w.PersonTypesId.Equals(personType.Id));
            }

            persons.ToList().ForEach(f => f.ActorsAge = f.GetActorsAge(f.Birthday, f.Obit));

            ViewData["OriginalFileDirectory"] = _originalFileDirectory;
            ViewData["FilterFor"] = GetNationalityName(id);

            return View("Index", persons);
        }

        // GET: Person/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .Include(i => i.PersonType)
                .Include(i => i.Sex)
                .Include(i => i.Nationality)
                .Include(i => i.PersonImages.OrderBy(o => o.Number))
                .Include(i => i.MoviesPerson)
                .ThenInclude(t => t.MovieRole)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            person.ActorsAge = person.GetActorsAge(person.Birthday, person.Obit);

            foreach (var mpId in person.MoviesPerson)
            {
                mpId.Movies = await _context.Movies.FirstOrDefaultAsync(f => f.Id == mpId.MoviesId);
                mpId.Movies.ActorsAge = mpId.Movies.GetActorsAgeInMovie(person.Birthday, mpId.Movies.YearOfPublication);
            }

            person.MoviesPerson = person.MoviesPerson.OrderBy(o => o.Movies.Name).ThenBy(t => t.Movies.YearOfPublication).ToList();

            ViewData["AdultPersonType"] = GetAdultPersonTypeId();
            ViewData["OriginalFileDirectory"] = _originalFileDirectory;
            ViewData["ImageSource"] = "";
            //if (!string.IsNullOrEmpty(person.Image))
            //    ViewData["ImageSource"] = $"{_originalFileDirectory}/{person.Image}";
            if (person.PersonImages != null && person.PersonImages.Count != 0)
            {
                var personImage = person.PersonImages.FirstOrDefault(f => f.IsMain == true);
                if (personImage != null)
                    ViewData["ImageSource"] = $"{_originalFileDirectory}/{personImage.PersonId}/{personImage.Name}";
            }
            return View(person);
        }

        public IActionResult Create()
        {
            ViewData["PersonTypesId"] = new SelectList(_context.PersonType, "Id", "Name");
            ViewData["SexId"] = new SelectList(_context.Sex.OrderBy(o => o.Name), "Id", "Name");
            ViewData["NationalityId"] = new SelectList(_context.Nationalities.OrderBy(o => o.Name), "Id", "Name");
            ViewData["AdultPersonType"] = GetAdultPersonTypeId();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,SexId,Birthday,Obit,NationalityId,Height,Weight,PersonTypesId,Classification,CupSize,FakeBoobs,StartOfBusiness,EndOfBusiness,SelectedFile")] PersonViewModel personViewModel)
        {
            Person person = new();

            if (ModelState.IsValid)
            {
                person = await MapAsync(personViewModel);
                if (personViewModel.SelectedFile != null)
                {
                    person.Image = SaveFile(personViewModel.SelectedFile, person.Id.ToString());
                }
                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PersonTypesId"] = new SelectList(_context.PersonType, "Id", "Name", personViewModel.PersonTypesId);
            ViewData["SexId"] = new SelectList(_context.Sex, "Id", "Name", personViewModel.SexId);
            ViewData["NationalityId"] = new SelectList(_context.Nationalities.OrderBy(o => o.Name), "Id", "Name", personViewModel.NationalityId);
            ViewData["AdultPersonType"] = GetAdultPersonTypeId();
            return View(personViewModel);
        }

        // GET: Person/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Person.Include(i => i.PersonImages.Where(w => w.IsDeleted == false).OrderBy(o => o.Number)).FirstOrDefaultAsync(f => f.Id.Equals(id));
            if (person == null)
            {
                return NotFound();
            }
            person.ActorsAge = person.GetActorsAge(person.Birthday, person.Obit);
            ViewData["PersonTypesId"] = new SelectList(_context.PersonType, "Id", "Name", person.PersonTypesId);
            ViewData["SexId"] = new SelectList(_context.Sex.OrderBy(o => o.Name), "Id", "Name", person.SexId);
            ViewData["NationalityId"] = new SelectList(_context.Nationalities.OrderBy(o => o.Name), "Id", "Name", person.NationalityId);
            ViewData["AdultPersonType"] = GetAdultPersonTypeId();
            ViewData["ImagePath"] = _originalFileDirectory;
            if (!string.IsNullOrEmpty(person.Image))
                ViewData["ImageSource"] = $"{_originalFileDirectory}/{person.Image}";
            else
                ViewData["ImageSource"] = string.Empty;

            var personEdit = new PersonViewModel(person)
            {
                PreviousPage = Request.Headers.Referer
            };
            return View(personEdit);
        }

        // POST: Person/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("Id,Name,SexId,Birthday,Obit,NationalityId,Height,Weight,PersonTypesId,Classification,CupSize,FakeBoobs,StartOfBusiness,EndOfBusiness,PreviousPage")] PersonViewModel personViewModel)
        {
            if (id != personViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var person = await MapAsync(personViewModel);
                    var mainImage = int.Parse(Request.Form["isMainImage"]);
                    if (personViewModel.SelectedFile != null)
                    {
                        var maxImageNumber = _context.PersonImage.Where(w => w.PersonId.Equals(id)).Max(m => m.Number) + 1;
                        var personImage = new PersonImage()
                        {
                            ChangedAt = DateTime.Now,
                            CreatetAt = DateTime.Now,
                            Id = Guid.NewGuid(),
                            IsDeleted = false,
                            IsMain = maxImageNumber == 1,
                            PersonId = id,
                            Name = SaveFile(personViewModel.SelectedFile, maxImageNumber.ToString(), person.Id),
                            Number = maxImageNumber
                        };
                        _context.PersonImage.Add(personImage);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        var personImage = await _context.PersonImage.FirstOrDefaultAsync(f => f.PersonId.Equals(id) && f.IsMain == true);
                        if (personImage != null && personImage.Number != mainImage)
                        {
                            personImage.IsMain = false;
                            _context.Update(personImage);
                            personImage = await _context.PersonImage.FirstOrDefaultAsync(f => f.PersonId.Equals(id) && f.Number == mainImage);
                            if (personImage != null)
                            {
                                personImage.IsMain = true;
                                _context.Update(personImage);
                            }
                        }
                    }

                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(personViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (!string.IsNullOrEmpty(personViewModel.PreviousPage))
                    return Redirect(personViewModel.PreviousPage);
                return RedirectToAction(nameof(Index));
            }
            ViewData["PersonTypesId"] = new SelectList(_context.PersonType, "Id", "Name", personViewModel.PersonTypesId);
            ViewData["SexId"] = new SelectList(_context.Sex, "Id", "Name", personViewModel.SexId);
            ViewData["NationalityId"] = new SelectList(_context.Nationalities.OrderBy(o => o.Name), "Id", "Name", personViewModel.NationalityId);
            ViewData["AdultPersonType"] = GetAdultPersonTypeId();
            ViewData["ImageSource"] = $"/{_originalFileDirectory}/{personViewModel.Image}";
            return View(personViewModel);
        }

        // GET: Person/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .Include(i => i.PersonType)
                .Include(i => i.Sex)
                .Include(i => i.Nationality)
                .Include(i => i.PersonImages)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: Person/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var person = await _context.Person.FindAsync(id);
            if (person != null)
            {
                var moviePersons = await _context.MoviesPerson.Where(w => w.PersonId.Equals(person.Id)).ToListAsync();
                if (moviePersons.Count != 0)
                {
                    foreach (var moviePerson in moviePersons)
                    {
                        var scenes = await _context.Scenes.Where(w => w.MoviesPersonsId.Equals(moviePerson.Id)).ToListAsync();
                        _context.Scenes.RemoveRange(scenes);
                    }
                    await _context.SaveChangesAsync();
                }
                _context.MoviesPerson.RemoveRange(moviePersons);
                await _context.SaveChangesAsync();
            }
            _context.Person.Remove(person);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Birthdays(string combine, DateTime birthday = new DateTime(), int year = 0)
        {
            if (birthday == DateTime.MinValue)
                birthday = DateTime.Today;

            List<Person> persons = [];

            if (year > 0)
            {
                persons = await _context.Person
                                        .Where(w => w.Birthday.Value.Year == year)
                                        .Include(i => i.PersonType)
                                        .Include(i => i.Sex)
                                        .Include(i => i.Nationality)
                                        .Include(i => i.MoviesPerson)
                                        .ThenInclude(t => t.MovieRole)
                                        .OrderBy(o => o.PersonType)
                                        .ThenBy(t => t.Name).ToListAsync();
                ViewData["BirthDay"] = $" im Jahr {year}";
            }
            else
            {
                persons = await _context.Person
                                        .Where(w => w.Birthday.Value.Month == birthday.Month && w.Birthday.Value.Day == birthday.Day)
                                        .Include(i => i.PersonType)
                                        .Include(i => i.Sex)
                                        .Include(i => i.Nationality)
                                        .Include(i => i.MoviesPerson)
                                        .ThenInclude(t => t.MovieRole)
                                        .OrderBy(o => o.PersonType)
                                        .ThenBy(t => t.Name).ToListAsync();
                ViewData["BirthDay"] = $" {birthday.Day}.{birthday.Month:00}.";
            }

            if (combine != null && combine.ToLower().Equals("on"))
            {
                if (year == 0) year = birthday.Year;
                persons = await _context.Person
                                        .Where(w => w.Birthday.Value.Year == year && w.Birthday.Value.Month == birthday.Month && w.Birthday.Value.Day == birthday.Day)
                                        .Include(i => i.PersonType)
                                        .Include(i => i.Sex)
                                        .Include(i => i.Nationality)
                                        .Include(i => i.MoviesPerson)
                                        .ThenInclude(t => t.MovieRole)
                                        .OrderBy(o => o.PersonType)
                                        .ThenBy(t => t.Name).ToListAsync();
                ViewData["BirthDay"] = $" {birthday.Day:00}.{birthday.Month:00}.{year}";
            }

            foreach (var person in persons)
            {
                person.ActorsAge = person.Obit == null ? (birthday.Year - person.Birthday.Value.Year).ToString() : person.GetActorsAge(person.Birthday, person.Obit);
                foreach (var id in person.MoviesPerson)
                {
                    id.Movies = await _context.Movies.FirstOrDefaultAsync(f => f.Id == id.MoviesId);
                    id.Movies.ActorsAge = id.Movies.GetActorsAgeInMovie(person.Birthday, id.Movies.YearOfPublication);
                }
                person.MoviesPerson = person.MoviesPerson.OrderBy(o => o.Movies.YearOfPublication).ThenBy(t => t.Movies.Name).Take(5).ToList();
            }
            //ViewData["BirthDay"] = birthday.ToShortDateString();
            if (!_showAdult)
                persons = persons.Where(w => !w.PersonType.Name.Contains("adult", StringComparison.CurrentCultureIgnoreCase)).ToList();

            ViewData["OriginalFileDirectory"] = _originalFileDirectory;
            return View(persons);
        }

        #endregion

        #region private functions

        private bool PersonExists(Guid id)
        {
            return _context.Person.Any(e => e.Id.Equals(id));
        }

        private string GetAdultPersonTypeId()
        {
            var personType = _context.PersonType.FirstOrDefault(f => f.Name.ToLower().Contains("adult"));
            if (personType != null)
                return personType.Id.ToString();
            return string.Empty;
        }

        private string GetNationalityName(Guid id)
        {
            if (id == Guid.Empty) return string.Empty;
            var nationality = _context.Nationalities.FirstOrDefault(f => f.Id.Equals(id));
            if (nationality != null)
                return $" für {nationality.Description}";
            return string.Empty;
        }

        private async Task<Person> MapAsync(PersonViewModel personViewModel)
        {
            var existsPerson = await _context.Person.FirstOrDefaultAsync(f => f.Id.Equals(personViewModel.Id));
            if (existsPerson != null)
            {
                existsPerson.Birthday = personViewModel.Birthday;
                existsPerson.Classification = personViewModel.Classification;
                existsPerson.CupSize = personViewModel.CupSize;
                existsPerson.EndOfBusiness = personViewModel.EndOfBusiness;
                existsPerson.FakeBoobs = personViewModel.FakeBoobs;
                existsPerson.Height = personViewModel.Height;
                existsPerson.Name = personViewModel.Name;
                existsPerson.NationalityId = personViewModel.NationalityId;
                existsPerson.Obit = personViewModel.Obit;
                existsPerson.PersonTypesId = personViewModel.PersonTypesId;
                existsPerson.SexId = personViewModel.SexId;
                existsPerson.StartOfBusiness = personViewModel.StartOfBusiness;
                existsPerson.Weight = personViewModel.Weight;
            }
            else
            {
                existsPerson = new Person()
                {
                    Birthday = personViewModel.Birthday,
                    Classification = personViewModel.Classification,
                    CupSize = personViewModel.CupSize,
                    EndOfBusiness = personViewModel.EndOfBusiness,
                    FakeBoobs = personViewModel.FakeBoobs,
                    Height = personViewModel.Height,
                    Id = Guid.NewGuid(),
                    Name = personViewModel.Name,
                    NationalityId = personViewModel.NationalityId,
                    Obit = personViewModel.Obit,
                    PersonTypesId = personViewModel.PersonTypesId,
                    SexId = personViewModel.SexId,
                    StartOfBusiness = personViewModel.StartOfBusiness,
                    Weight = personViewModel.Weight,
                };
            }
            return existsPerson;
        }

        private string SaveFile(IFormFile file, string newFileName)
        {
            if (file == null) return string.Empty;

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, _originalFilePath);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            var originalFileExtension = Path.GetExtension(file.FileName);
            newFileName += originalFileExtension;
            filePath = Path.Combine(filePath, newFileName);

            using var stream = System.IO.File.Create(filePath);
            file.CopyTo(stream);

            return newFileName;
        }

        private string SaveFile(IFormFile file, string newFileName, Guid personId)
        {
            if (file == null) return string.Empty;

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, _originalFilePath, personId.ToString());
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            var maxImageNumber = _context.PersonImage.Where(w => w.PersonId.Equals(personId)).Max(m => m.Number) + 1;

            var originalFileExtension = Path.GetExtension(file.FileName);
            newFileName += originalFileExtension;
            filePath = Path.Combine(filePath, newFileName);

            using var stream = System.IO.File.Create(filePath);
            file.CopyTo(stream);

            return newFileName;
        }

        #endregion
    }
}