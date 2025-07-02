namespace MvcMoviesCore.Interfaces
{
    public interface IBMI
    {
        public decimal? Value { get; set; }
        public decimal? GetBMI(decimal? height, decimal? weight, int decimals = 0);
        public decimal? GetBMI(int? feet, int? inch, decimal? lbs, int decimals = 0);
    }
}