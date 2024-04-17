using Microsoft.AspNetCore.Mvc;

namespace MvcMoviesCore.ApiController
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ApiMoviesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}