namespace MvcMoviesCore.Classes
{
    public class DiagramData
    {
        public DataSet[] DataSets { get; set; } = [];
        public string[] Labels { get; set; } = [];
    }

    public class DataSet
    {
        public string Type { get; set; } = "pie";
        public string Label { get; set; }
        public int[] Data { get; set; } = [];
        public string[] BackgroundColor { get; set; } = [];
    }
}