using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesApiController : ControllerBase
    {
        private readonly MvcMovieCoreContext _context;

        public MoviesApiController(MvcMovieCoreContext context) 
        { 
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IEnumerable<MoviesPerson>> GetMoviePerson(Guid id)
        {
            return await _context.MoviesPerson.Where(w => w.MoviesId.Equals(id)).ToListAsync();
        }

        [HttpPost("{id}")]
        //[HttpPost, ActionName("DeleteLink")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLink(Guid id)
        {
            var moviePerson = await _context.MoviesPerson.FindAsync(id);
            if (moviePerson != null)
            {
                var scenes = _context.Scenes.Where(w => w.MoviesPersonsId.Equals(moviePerson.Id)).ToList();
                if (scenes.Any())
                {
                    _context.Scenes.RemoveRange(scenes);
                    await _context.SaveChangesAsync();
                }
                _context.MoviesPerson.Remove(moviePerson);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}