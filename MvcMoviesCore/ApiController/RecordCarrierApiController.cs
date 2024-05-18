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
    public class RecordCarrierApiController : ControllerBase
    {

        private readonly MvcMovieCoreContext _context;

        public RecordCarrierApiController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Get()
        {
            var recordCarriers = await _context.RecordCarrier.ToListAsync();
            if (recordCarriers.Count != 0)
            {
                var movies = _context.Movies.GroupBy(g => g.RecordCarrierId).ToList();
                foreach (var movie in movies)
                {
                    var recordCarrier = recordCarriers.FirstOrDefault(f => f.Id.Equals(movie.Key));
                    if (recordCarrier != null)
                        recordCarrier.Count = movie.Count();
                }
            }
            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(recordCarriers, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var recordCarrier = await _context.RecordCarrier.FirstOrDefaultAsync(f=>f.Id.Equals(id));

            if (recordCarrier == null)
                return BadRequest("Datenträger nicht gefunden!");

            if (await _context.Movies.AnyAsync(a => a.RecordCarrierId.Equals(recordCarrier.Id)))
                return BadRequest("Datenträger wird verwendet und kann daher nicht gelöscht werden!");

            _context.RecordCarrier.Remove(recordCarrier);
            _context.SaveChanges();

            return Ok();
        }
    }
}