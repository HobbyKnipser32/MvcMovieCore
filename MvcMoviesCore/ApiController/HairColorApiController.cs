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
    public class HairColorApiController(MvcMovieCoreContext context) : ControllerBase
    {

        private readonly MvcMovieCoreContext _context = context;

        public async Task<IActionResult> Get()
        {
            List<HairColor> hairColors = await _context.HairColors.OrderBy(o => o.Color).ToListAsync();

            if (hairColors.Count != 0)
            {
                var persons = _context.Person.Where(w => w.HairColorId != null).GroupBy(g => g.HairColorId).ToList();
                foreach (var person in persons)
                {
                    var hairColor = hairColors.FirstOrDefault(f => f.Id.Equals(person.Key));
                    if (hairColor != null)
                        hairColor.Count = person.Count();
                }
            }
            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(hairColors, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var hairColor = await _context.HairColors.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (hairColor == null)
                return BadRequest("Konnte Haarfarbe nicht finden!");

            if (IsHairColorUsed(id))
                return BadRequest("Haarfarbe wird verwendet und kann daher nicht gelöscht werden!");

            _context.HairColors.Remove(hairColor);
            _context.SaveChanges();

            return Ok();
        }

        #region private fields

        private bool IsHairColorUsed(Guid id)
        {
            return _context.Person.Any(a => a.HairColorId.Equals(id));
        }

        #endregion
    }
}