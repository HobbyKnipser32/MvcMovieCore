using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Models;
using MvcMoviesCore.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleApiController(MvcMovieCoreContext context) : ControllerBase
    {

        private readonly MvcMovieCoreContext _context = context;

        public async Task<IActionResult> Get()
        {
            var roles = await _context.MovieRole.OrderBy(o => o.Name).ToListAsync();
            var movieRoles = new List<MovieRoleViewModel>();

            if (roles.Count != 0)
            {
                foreach (var role in roles) 
                { 
                    var movies = await _context.MoviesPerson.Where(w=>w.MovieRoleId.Equals(role.Id)).ToListAsync();
                    MovieRoleViewModel movieRole = new() 
                    { 
                        Count = movies.Count,
                        Id = role.Id,
                        Name = role.Name,
                    };
                    movieRoles.Add(movieRole);
                }
            }
            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(movieRoles, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var role = await _context.MovieRole.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (role == null)
                return BadRequest("Konnte Genre nicht finden!");

            if (IsGenreUsed(id))
                return BadRequest("Genre wird verwendet und kann daher nicht gelöscht werden!");

            _context.MovieRole.Remove(role);
            await _context.SaveChangesAsync();

            return Ok();
        }

        #region private fields

        private bool IsGenreUsed(Guid id)
        {
            return _context.MoviesPerson.Any(a => a.MovieRoleId.Equals(id));
        }

        #endregion
    }
}