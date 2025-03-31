using System;

namespace MvcMoviesCore.ViewModels
{
    public class SearchMovieViewModel
    {
        public string Title { get; set; }
        public int? YearOfPuplication {  get; set; }
        public int? RunTimeFrom { get; set; }
        public int? RunTimeTo { get; set; }
        public Guid? Genre { get; set; }
        public Guid? RecordCarrier { get; set; }
    }
}