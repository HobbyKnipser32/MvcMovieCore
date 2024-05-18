using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;


namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreApiController : ControllerBase
    {

        private readonly MvcMovieCoreContext _context;
        private readonly IConfiguration _configuration;
        private readonly bool _showAdult;

        public GenreApiController(MvcMovieCoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _showAdult = _configuration.GetValue<bool>("AppSettings:ShowAdult");
        }

        public async Task<IActionResult> Get()
        {
            List<Genre> genres = [];

            if (_showAdult)
                genres = await _context.Genre.ToListAsync();
            else
                genres = await _context.Genre.Where(w => w.IsAdult == false).ToListAsync();

            if (genres.Count != 0)
            {
                var movies = _context.Movies.GroupBy(g => g.GenreId).ToList();
                foreach (var movie in movies)
                {
                    var genre = genres.FirstOrDefault(f => f.Id.Equals(movie.Key));
                    if (genre != null)
                        genre.Count = movie.Count();
                }
            }
            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(genres, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var genre = await _context.Genre.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (genre == null)
                return BadRequest("Konnte Genre nicht finden!");

            if (IsGenreUsed(id))
                return BadRequest("Genre wird verwendet und kann daher nicht gelöscht werden!");

            _context.Genre.Remove(genre);
            _context.SaveChanges();

            return Ok();
        }

        #region private fields

        private bool IsGenreUsed(Guid id)
        {
            return _context.Movies.Any(a => a.GenreId.Equals(id));
        }

        #endregion
    }
}