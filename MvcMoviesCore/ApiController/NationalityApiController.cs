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
//using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class NationalityApiController : ControllerBase
    {

        private readonly MvcMovieCoreContext _context;

        public NationalityApiController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Get()
        {
            var nationalities = await _context.Nationalities.OrderBy(o => o.Description).ToListAsync();
            if (nationalities.Count != 0)
            {
                var persons = _context.Person.GroupBy(g => g.NationalityId).ToList();
                foreach (var person in persons)
                {
                    var nationality = nationalities.FirstOrDefault(f => f.Id.Equals(person.Key));
                    if (nationality != null)
                        nationality.Count = person.Count();
                }
            }
            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(nationalities, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var nationality = await _context.Nationalities.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (nationality == null)
                return BadRequest("Kann Nationalität nicht finden!");

            if (IsNationalityUsed(id))
                return BadRequest("Nationalität wird verwendet und kann daher nicht gelöscht werden!");

            _context.Nationalities.Remove(nationality);
            _context.SaveChanges();

            return Ok();
        }

        #region private fields

        private bool IsNationalityUsed(Guid id)
        {
            return _context.Person.Any(a => a.NationalityId.Equals(id));
        }

        #endregion
    }
}