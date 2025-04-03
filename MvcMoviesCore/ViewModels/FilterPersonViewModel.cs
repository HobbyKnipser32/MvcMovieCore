using System;

namespace MvcMoviesCore.ViewModels
{
    public class FilterPersonViewModel
    {
        public string Name {  get; set; }
        public int? YearOfBirth { get; set; }
        public Guid? Nationality { get; set; }
        public string HeightFrom { get; set; }
        public string HeightTo { get; set; }
        public string WeightFrom {  get; set; }
        public string WeightTo { get; set; }
        public Guid? PersonType { get; set; }
        public Guid? Sex { get; set; }
    }
}