namespace MinimalApiMovies.Entities
{
    public class GenreMovie
    {
        public int MovieId { get; set; }

        public int GenreId { get; set; }

        public Genre genre { get; set; } = null!;

        public Movies movie { get; set; } = null!;
    }
}
