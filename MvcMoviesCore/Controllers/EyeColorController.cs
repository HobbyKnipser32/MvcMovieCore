using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MvcMoviesCore.Models;
using System;
using System.Linq;

namespace MvcMoviesCore.Controllers
{
    public class EyeColorController(MvcMovieCoreContext context) : Controller
    {
        private readonly MvcMovieCoreContext _context = context;

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                string color = collection["Color"];
                string description = collection["Description"];
                if (!ExistsColor(color))
                {
                    var eyeColor = new EyeColor { Id = Guid.NewGuid(), Color = color.ToLower() };
                    if (!string.IsNullOrEmpty(description))
                        eyeColor.Description = description;
                    _context.EyeColors.Add(eyeColor);
                    _context.SaveChanges();
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(Guid id)
        {
            if (id == Guid.Empty)
                return RedirectToAction(nameof(Index));

            var eyeColor = _context.EyeColors.FirstOrDefault(f => f.Id.Equals(id));
            if (eyeColor == null)
                return RedirectToAction(nameof(Index));

            return View(eyeColor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id, IFormCollection collection)
        {
            try
            {
                string color = collection["Color"];
                string description = collection["Description"];
                var eyeColor = _context.EyeColors.FirstOrDefault(f => f.Id.Equals(id));
                eyeColor.Color = color;
                if (!string.IsNullOrEmpty(description))
                    eyeColor.Description = description;
                _context.EyeColors.Update(eyeColor);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        #region private functions

        private bool ExistsColor(string color)
        {
            return _context.EyeColors.Any(a => a.Color.Equals(color.ToLower()));
        }

        #endregion
    }
}
