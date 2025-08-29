using System;

namespace MvcMoviesCore.ViewModels
{
    public class PracticeFilterViewModel
    {
        public Guid PersonId { get; set; }
        public string[] CheckedValues { get; set; }
    }
}