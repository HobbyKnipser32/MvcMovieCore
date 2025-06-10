namespace MvcMoviesCore.Interfaces
{
    public interface IBMI
    {
        public decimal? Value { get; set; }
        public decimal? GetBMI(decimal? height, decimal? weight);
        public decimal? GetBMI(int? feet, int? inch, decimal? lbs);
    }
}