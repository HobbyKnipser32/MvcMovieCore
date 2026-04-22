using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMoviesCore.Classes;
using MvcMoviesCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MvcMoviesCore.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class EyeColorApiController(MvcMovieCoreContext context) : ControllerBase
    {

        private readonly MvcMovieCoreContext _context = context;

        public async Task<IActionResult> Get()
        {
            List<EyeColor> eyeColors = await _context.EyeColors.OrderBy(o => o.Color).ToListAsync();

            if (eyeColors.Count != 0)
            {
                var persons = _context.Person.Where(w => w.EyeColorId != null).GroupBy(g => g.EyeColorId).ToList();
                foreach (var person in persons)
                {
                    var eyeColor = eyeColors.FirstOrDefault(f => f.Id.Equals(person.Key));
                    eyeColor?.Count = person.Count();
                }
            }
            var withoutEyeColor = _context.Person.Count(c => c.EyeColorId == null);
            var eyeColorCount = new EyeColor() { Id = Guid.NewGuid(), Color = "Personen ohne Augenfarbe", Count = withoutEyeColor };
            eyeColors.Add(eyeColorCount);
            var jsonSerializerSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var jsonResult = JsonConvert.SerializeObject(eyeColors, Formatting.Indented, jsonSerializerSettings);
            return Ok(jsonResult);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var eyeColor = await _context.EyeColors.FirstOrDefaultAsync(f => f.Id.Equals(id));

            if (eyeColor == null)
                return BadRequest("Konnte Augenfarbe nicht finden!");

            if (IsEyeColorUsed(id))
                return BadRequest("Augenfarbe wird verwendet und kann daher nicht gelöscht werden!");

            _context.EyeColors.Remove(eyeColor);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("GetDiagramData")]
        public async Task<IActionResult> GetDiagramData()
        {
            var eyeColors = await _context.EyeColors.OrderBy(o => o.Color).ToListAsync();
            var persons = _context.Person.Where(w => w.EyeColorId != null).GroupBy(g => g.EyeColorId).ToList();
            List<DataSet> dataSets = [];
            var dataSet = new DataSet() { Label = "Augenfarben" }; //, Type = "polarArea" };
            List<string> labels = [];
            List<int> datas = [];
            List<string> backgroundColors = [];
            foreach (var eyeColor in eyeColors)
            {
                labels.Add(eyeColor.Color);
                datas.Add(_context.Person.Count(c => c.EyeColorId.Equals(eyeColor.Id)));
                backgroundColors.Add(GetEyeColor(eyeColor.Color));
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