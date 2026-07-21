namespace MinimalApiMovies.Entities
{
    public class Movies
    {

        public int Id { get; set; }
        public string Title { get; set; } = null!;

        public bool InTheaters { get; set; }

        public DateTime ReleaseDate {  get; set; }

        public string? Poster { get; set; }

        public List<Comments> Comments { get; set; } = new List<Comments>();

        public List<GenreMovie> GenresMovies { get; set; } = new List<GenreMovie>();
        public List<ActorMovie> ActorsMovies { get; set; } = new List<ActorMovie>();


    }
}
