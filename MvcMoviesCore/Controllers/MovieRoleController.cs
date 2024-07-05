using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMoviesCore.Models;
using MvcMoviesCore.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MvcMoviesCore.Controllers
{
    public class MovieRoleController : Controller
    {
        #region privat fields

        private readonly MvcMovieCoreContext _context;
        private readonly IConfiguration _configuration;
        private readonly bool _showAdult;
        private readonly string _originalFileDirectory = "Images/Original";

        #endregion

        #region constructor

        public MovieRoleController(MvcMovieCoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _showAdult = _configuration.GetValue<bool>("AppSettings:ShowAdult");
            _originalFileDirectory = _configuration.GetValue<string>("AppSettings:OriginalFileDirectory");
        }

        #endregion

        #region public functions

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(Guid id)
        {
            var role = _context.MovieRole.FirstOrDefault(x => x.Id.Equals(id));
            if (role == null)
                return NotFound();
            var moviePersons = _context.MoviesPerson
                .Include(i => i.Movies)
                .Include(i => i.Person)
                .Where(w => w.MovieRole.Id.Equals(id))
                .OrderBy(o=>o.Person.Name)
                .ThenBy(o => o.Movies.Name)
                .ToList();
            return View(moviePersons);
        }


        #endregion

        #region private functions

        #endregion
    }
}