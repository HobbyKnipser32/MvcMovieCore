﻿using MvcMoviesCore.Models;

namespace MvcMoviesCore.ViewModels
{
    public class PersonSceneViewModel
    {
        public string Person { get; set; }
        public Movies Film { get; set; }
        public string? Szene { get; set; }
    }
}