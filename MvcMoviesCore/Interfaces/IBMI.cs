namespace MvcMoviesCore.Interfaces
{
    public interface IBMI
    {
        public int? Value { get; set; }
        public int? GetBMI(decimal? height, decimal? weight);
        public int? GetBMI(int? feet, int? inch, decimal? lbs);
    }
}