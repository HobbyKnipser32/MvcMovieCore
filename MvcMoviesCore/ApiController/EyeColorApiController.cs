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
    public class EyeColorApiController(MvcMovieCoreContext context) : ControllerBase
    {

        private readonly MvcMovieCoreContext _context = context;

        public async Task<IActionResult> Get()
        {
            List<EyeColor> eyeColors = await _context.EyeColors.OrderBy(o => o.Color).ToListAsync();

            if (eyeColors.Count != 0)
            {
                var persons = _context.Person.Where(w => w.EyeColorId != null).GroupBy(g => g.EyeColorId).ToList();
                foreach (var person in persons)
                {
                    var eyeColor = eyeColors.FirstOrDefault(f => f.Id.Equals(person.Key));
                    if (eyeColor != null)
                        eyeColor.Count = person.Count();
                }
            }
            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(eyeColors, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var eyeColor = await _context.EyeColors.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (eyeColor == null)
                return BadRequest("Konnte Augenfarbe nicht finden!");

            if (IsEyeColorUsed(id))
                return BadRequest("Augenfarbe wird verwendet und kann daher nicht gelöscht werden!");

            _context.EyeColors.Remove(eyeColor);
            _context.SaveChanges();

            return Ok();
        }

        #region private fields

        private bool IsEyeColorUsed(Guid id)
        {
            return _context.Person.Any(a => a.EyeColorId.Equals(id));
        }

        #endregion
    }
}