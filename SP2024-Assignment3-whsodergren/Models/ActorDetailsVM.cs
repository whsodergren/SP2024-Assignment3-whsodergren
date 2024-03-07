namespace SP2024_Assignment3_whsodergren.Models
{
    public class ActorDetailsVM
    {
        public Actor actor {  get; set; }
        public List<Movie> movies { get; set; }
        public string Sentiment { get; set; }
        public List<WikiPost> WikiPosts { get; set; }
    }
}
