using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Classes;
using MvcMoviesCore.Models;
using Newtonsoft.Json;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class HairColorApiController(MvcMovieCoreContext context) : ControllerBase
    {

        private readonly MvcMovieCoreContext _context = context;

        public async Task<IActionResult> Get()
        {
            List<HairColor> hairColors = await _context.HairColors.OrderBy(o => o.Color).ToListAsync();

            if (hairColors.Count != 0)
            {
                var persons = _context.Person.Where(w => w.HairColorId != null).GroupBy(g => g.HairColorId).ToList();
                foreach (var person in persons)
                {
                    var hairColor = hairColors.FirstOrDefault(f => f.Id.Equals(person.Key));
                    hairColor?.Count = person.Count();
                }
            }

            var withoutHairColor = _context.Person.Count(c => c.HairColorId == null);
            var hairColorCount = new HairColor() { Id = Guid.NewGuid(), Color = "Personen ohne Haarfarbe", Count = withoutHairColor };
            hairColors.Add(hairColorCount);

            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(hairColors, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var hairColor = await _context.HairColors.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (hairColor == null)
                return BadRequest("Konnte Haarfarbe nicht finden!");

            if (IsHairColorUsed(id))
                return BadRequest("Haarfarbe wird verwendet und kann daher nicht gelöscht werden!");

            _context.HairColors.Remove(hairColor);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("GetDiagramData")]
        public async Task<IActionResult> GetDiagramData()
        {
            var hairColors = await _context.HairColors.OrderBy(o => o.Color).ToListAsync();
            var persons = _context.Person.Where(w => w.HairColorId != null).GroupBy(g => g.HairColorId).ToList();
            List<DataSet> dataSets = [];
            var dataSet = new DataSet() { Label = "Haarfarben" };
            List<string> labels = [];
            List<int> datas = [];
            List<string> backgroundColors = [];
            foreach (var hairColor in hairColors)
            {
                labels.Add(hairColor.Color);
                datas.Add(_context.Person.Count(c => c.HairColorId.Equals(hairColor.Id)));
                backgroundColors.Add(GetHairColor(hairColor.Color));
            }

            DiagramData diagramData = new()
            {
                Labels = [.. labels],
            };
            dataSet.Data = [.. datas];
            dataSet.BackgroundColor = [.. backgroundColors];
            dataSets.Add(dataSet);
            diagramData.DataSets = [.. dataSets];

            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(diagramData, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        #region private fields

        private bool IsHairColorUsed(Guid id)
        {
            return _context.Person.Any(a => a.HairColorId.Equals(id));
        }

        private string GetHairColor(string color)
        {
            string hairColor = string.Empty;
            switch (color.ToLower())
            {
                case "blond":
                    hairColor = "rgb(217,179,128)";
                    break;
                case "braun":
                    hairColor = "rgb(124,84,56)";
                    break;
                case "dunkelblond":
                    hairColor = "rgb(179,139,103)";
                    break;
                case "dunkelbraun":
                    hairColor = "rgb(84,50,48)";
                    break;
                case "grau":
                    hairColor = "rgb(169,169,169)";
                    break;
                case "hellbraun":
                    hairColor = "rgb(169,125,57)";
                    break;
                case "honigblond":
                    hairColor = "rgb(255, 230, 158)";
                    break;
                case "kahl":
                    hairColor = "rgb(252,203,173)";
                    break;
                case "rehbraun":
                    hairColor = "rgb(237,147,82)";
                    break;
                case "rot":
                    hairColor = "rgb(182,51,51)";
                    break;
                case "rotblond":
                    hairColor = "rgb(245, 231, 219)";
                    break;
                case "schwarz":
                    hairColor = "rgb(32,32,36)";
                    break;
                default:
                    hairColor = "rgb(127,127,127)";
                    break;
            }
            return hairColor;
        }

        #endregion
    }
}