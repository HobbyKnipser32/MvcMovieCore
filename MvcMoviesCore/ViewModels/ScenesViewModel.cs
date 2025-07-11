using System;

namespace MvcMoviesCore.ViewModels
{
    public class ScenesViewModel
    {
        public Guid PersonId { get; set; }
        public int Nr { get; set; } = 0;
        public string Sex { get; set; }
        public string Name { get; set; }
        public int? Classification { get; set; }
    }
}