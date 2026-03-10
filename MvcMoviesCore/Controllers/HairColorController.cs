using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MvcMoviesCore.Models;
using System;
using System.Linq;

namespace MvcMoviesCore.Controllers
{
    public class HairColorController(MvcMovieCoreContext context) : Controller
    {
        private readonly MvcMovieCoreContext _context = context;

        public ActionResult Index()
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
                    var haircolor = new HairColor { Id = Guid.NewGuid(), Color = color.ToLower() };
                    if (!string.IsNullOrEmpty(description))
                        haircolor.Description = description;
                    _context.HairColors.Add(haircolor);
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

            var hairColor = _context.HairColors.FirstOrDefault(f => f.Id.Equals(id));
            if (hairColor == null)
                return RedirectToAction(nameof(Index));

            return View(hairColor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid id, IFormCollection collection)
        {
            try
            {
                string color = collection["Color"];
                string description = collection["Description"];
                if (!ExistsColor(color))
                {
                    var haircolor = _context.HairColors.FirstOrDefault(f => f.Id.Equals(id));
                    haircolor.Color = color;
                    if (!string.IsNullOrEmpty(description))
                        haircolor.Description = description;
                    _context.HairColors.Update(haircolor);
                    _context.SaveChanges();
                }
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
            return _context.HairColors.Any(a => a.Color.Equals(color.ToLower()));
        }

        #endregion
    }
}
