using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Classes;
using MvcMoviesCore.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeApiController(MvcMovieCoreContext context) : ControllerBase
    {

        private readonly MvcMovieCoreContext _context = context;

        public async Task<IActionResult> Get()
        {
            var practices = await _context.Practices.OrderBy(o => o.Praxis).ToListAsync();

            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(practices, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var practice = await _context.Practices.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (practice == null)
                return BadRequest("Konnte Praxis nicht finden!");


            _context.Practices.Remove(practice);
            _context.SaveChanges();

            return Ok();
        }


        #region private functions

        private bool IsEyeColorUsed(Guid id)
        {
            return _context.Person.Any(a => a.EyeColorId.Equals(id));
        }

        private string GetEyeColor(string color)
        {
            string eyeColor = color.ToLower() switch
            {
                "blau" => "rgb(75,146,219)",
                "braun" => "rgb(139,69,19)",
                "grau" => "rgb(127,127,127)",
                "grau-blau" => "rgb(93,105,112)",
                "grün" => "rgb(50,205,50)",
                "rehbraun" => "rgb(205,133,63)",
                _ => "rgb(255,255,255)",
            };
            return eyeColor;
        }

        #endregion
    }
}