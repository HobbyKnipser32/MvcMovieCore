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
    public class PersonTypesApiController : ControllerBase
    {
        #region fields

        private readonly MvcMovieCoreContext _context;

        #endregion

        #region constructors

        public PersonTypesApiController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        #endregion

        #region public fields

        public async Task<IActionResult> Get()
        {
            var personTypes = await _context.PersonType.ToListAsync();
            if (personTypes.Count != 0)
            {
                var persons = await _context.Person.GroupBy(g => g.PersonTypesId).ToListAsync();
                foreach (var person in persons)
                {
                    var personType = personTypes.FirstOrDefault(f => f.Id.Equals(person.Key));
                    if (personType != null)
                        personType.Count = person.Count();
                }
            }
            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(personTypes, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var personType = await _context.PersonType.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (personType == null) 
            {
                return BadRequest("Konnte Personentyp nicht finden.");
            }
            if (IsPersonTypeUsed(id))
            {
                return BadRequest("Personentyp kann nicht gelöscht werden.");
            }
            _context.PersonType.Remove(personType);
            _context.SaveChanges();

            return Ok();
        }

        #endregion

        #region private fields

        private bool IsPersonTypeUsed(Guid id)
        {
            return _context.Person.Any(a => a.PersonTypesId.Equals(id));
        }

        #endregion
    }
}