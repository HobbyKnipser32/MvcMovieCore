using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMoviesCore.Controllers
{
    public class GetRandomMovieController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}