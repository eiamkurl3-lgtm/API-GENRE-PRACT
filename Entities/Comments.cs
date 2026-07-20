namespace MinimalApiMovies.Entities
{
    public class Comments
    {
        public int id { get; set; }
        public string Body { get; set; } = null!;
        public int MovieId { get; set; }
    }
}
