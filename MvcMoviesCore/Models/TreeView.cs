namespace MvcMoviesCore.Models
{
    public class TreeView
    {
        public string Header { get; set; }

        public string Image { get; set; }

        public bool NewItemy { get; set; }

        public TreeView[] Items { get; set; }

        public static TreeView[] GetTreeView(string val)
        {
            return new TreeView[]
            {
                new TreeView
                {
                    Header = "Movies",
                },
                new TreeView
                {
                    Header = "Cast & Crew",
                    Items = new TreeView[]
                    {
                        new TreeView { Header = "Director"},
                        new TreeView { Header = "Actor"},
                        new TreeView { Header = "Writer"}
                    }
                },
                new TreeView
                {
                    Header = "Misc",
                    Items = new TreeView[]
                    {
                        new TreeView { Header = "Genre"},
                        new TreeView { Header = "Person Types"},
                        new TreeView { Header = "RecordCarrier"},
                        new TreeView { Header = "Sex"},
                        new TreeView { Header = "StorageLocation"}
                    }
                }
            };
        }
    }
}