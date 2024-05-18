using Microsoft.AspNetCore.Mvc;
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
    public class StorageLocationApiController : ControllerBase
    {

        private readonly MvcMovieCoreContext _context;

        public StorageLocationApiController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Get()
        {
            var storageLocations = await _context.StorageLocation.ToListAsync();
            if (storageLocations.Count != 0)
            {
                var movies = await _context.Movies.GroupBy(g => g.StorageLocationId).ToListAsync();
                foreach (var movie in movies)
                {
                    var storageLocation = storageLocations.FirstOrDefault(f => f.Id.Equals(movie.Key));
                    if (storageLocation != null)
                        storageLocation.Count = movie.Count();
                }
            }
            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(storageLocations, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var storageLocation = await _context.StorageLocation.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (storageLocation == null)
                return BadRequest("Lagerort nicht gefunden!");

            if (await _context.Movies.AnyAsync(a => a.StorageLocationId.Equals(storageLocation.Id)))
                return BadRequest("Lagerort wird verwendet und kann daher nicht gelöscht werden!");

            _context.StorageLocation.Remove(storageLocation);
            _context.SaveChanges();

            return Ok();
        }
    }
}