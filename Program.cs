using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.OutputCaching;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Services zone - BEGIN

builder.Services.AddScoped<IGenreRepository, GenreRepository>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(configuration =>
    {
        configuration.WithOrigins(builder.Configuration["allowedOrigins"]!).AllowAnyMethod()
        .AllowAnyHeader();
    });

    options.AddPolicy("free", configuration =>
    {
        configuration.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddOutputCache();
//builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services zone - END

var app = builder.Build();
// Middlewares zone - BEGIN

//if (builder.Environment.IsDevelopment())
//{

//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseOutputCache();

app.MapGet("/", () => "Hello, world");


//app.MapGet("/genres", () =>
//{
//    var genres = new List<MinimalApiMovies.Entities.Genre>
//    {
//        new MinimalApiMovies.Entities.Genre { Id = 1, Name = "Action", Description = "Action movies" },
//        new MinimalApiMovies.Entities.Genre { Id = 2, Name = "Comedy", Description = "Comedy movies" },
//        new MinimalApiMovies.Entities.Genre { Id = 3, Name = "Drama", Description = "Drama movies" }
//    };
//    return genres;
//});

//app.MapGet("/genres",[EnableCors(policyName:"free")]() =>
//{
//    var genres = new List<MinimalApiMovies.Entities.Genre>
//    {
//        new MinimalApiMovies.Entities.Genre { Id = 1, Name = "Action", Description = "Action movies" },
//        new MinimalApiMovies.Entities.Genre { Id = 2, Name = "Comedy", Description = "Comedy movies" },
//        new MinimalApiMovies.Entities.Genre { Id = 3, Name = "Drama", Description = "Drama movies" }
//    };
//    return genres;
//});

app.MapPost("/Genre", async (Genre genre, IGenreRepository repository,
    IOutputCacheStore outputCacheStore) =>
{
    var id = await repository.Create(genre);
    await outputCacheStore.EvictByTagAsync("genre-get", default);
    return TypedResults.Created($"/Genre/{genre.Id}", genre);
});

app.MapGet("/Genre", async (IGenreRepository GenreRepository) =>
{
    await GenreRepository.GetAll();
}).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(15)).Tag("genre-get"));

app.MapGet("/Genre/{id:int}", async (int id, IGenreRepository repository) =>
{
    var genre = await repository.GetById(id);

    if (genre == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(genre);
});
//middleware zone end
app.Run();
