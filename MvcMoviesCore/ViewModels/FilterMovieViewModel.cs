﻿using System;

namespace MvcMoviesCore.ViewModels
{
    public class FilterMovieViewModel
    {
        public string Title { get; set; }
        public int? YearOfPuplication {  get; set; }
        public int? RunTimeFrom { get; set; }
        public int? RunTimeTo { get; set; }
        public int? Ranking { get; set; }
        public Guid? Genre { get; set; }
        public Guid? RecordCarrier { get; set; }
        public string Marker {  get; set; }
    }
}