

using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);

//services

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(configuration =>
    {
        configuration.WithOrigins(builder.Configuration["allowedOrigins"]).AllowAnyHeader().AllowAnyMethod();
    });
       options.AddPolicy("Free", configuration =>
       configuration.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});


builder.Services.AddOutputCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//services end


var app = builder.Build();
//middleware zone begin

//if (builder.Environment.IsDevelopment())
//{
    
//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseOutputCache();

app.UseCors();

app.MapGet("/", () => "firstname");


app.MapGet("/genres", () =>
{
    var genres = new List<MinimalApiMovies.Entities.Genre>
    {
        new MinimalApiMovies.Entities.Genre { Id = 1, Name = "Action", Description = "Action movies" },
        new MinimalApiMovies.Entities.Genre { Id = 2, Name = "Comedy", Description = "Comedy movies" },
        new MinimalApiMovies.Entities.Genre { Id = 3, Name = "Drama", Description = "Drama movies" }
    };
    return genres;
});

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


//middleware zone end
app.Run();
