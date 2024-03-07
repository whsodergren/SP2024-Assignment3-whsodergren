namespace SP2024_Assignment3_whsodergren.Models
{
    public class MovieDetailsVM
    {
        public Movie movie { get; set; }
        public List<Actor> actors { get; set; }
        public string Sentiment { get; set; }
        public List<WikiPost> WikiPosts { get; set; }


    }
}
