﻿using Microsoft.AspNetCore.Mvc;
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
    public class SexApiController : ControllerBase
    {

        private readonly MvcMovieCoreContext _context;

        public SexApiController(MvcMovieCoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Get()
        {
            var sexs = await _context.Sex.ToListAsync();
            if (sexs.Count != 0)
            {
                var persons = await _context.Person.GroupBy(g => g.SexId).ToListAsync();
                foreach (var person in persons)
                {
                    var sex = sexs.FirstOrDefault(f => f.Id.Equals(person.Key));
                    if (sex != null)
                        sex.Count = person.Count();
                }
            }
            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(sexs, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var sex = await _context.Sex.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (sex == null)
                return BadRequest("Geschlecht nicht gefunden!");

            if (await _context.Person.AnyAsync(a => a.SexId.Equals(sex.Id)))
                return BadRequest("Geschlecht wird verwendet und kann daher nicht gelöscht werden!");

            _context.Sex.Remove(sex);
            _context.SaveChanges();

            return Ok();
        }
    }
}