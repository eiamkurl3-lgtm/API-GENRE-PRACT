using System.ComponentModel;

namespace MinimalApiMovies.Services
{
    public interface IFileStorage
    {
        Task<String> Store(string container, IFormFile file);
        Task Delete(string? route, string container);
        async Task<string> Edit(string? route, string container, IFormFile file)
        { 
            await Delete(route, container);
            return await Store(container, file);
        }
    }
}
