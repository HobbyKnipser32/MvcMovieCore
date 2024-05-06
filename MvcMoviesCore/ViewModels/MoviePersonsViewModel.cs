using System;

namespace MvcMoviesCore.ViewModels
{
    public class MoviePersonsViewModel
    {
        public string Actor {  get; set; }  
        public string Role { get; set; }
        public Guid MovieRoleId { get; set; }
    }
}