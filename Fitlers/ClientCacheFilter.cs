using Microsoft.AspNetCore.Http;

namespace MinimalApiMovies.Fitlers
{
    public class ClientCacheFilter : IEndpointFilter
    {
        private readonly int _maxAge;

        public ClientCacheFilter(int maxAge)
        {
            _maxAge = maxAge;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            context.HttpContext.Response.Headers.CacheControl = $"public, max-age={_maxAge}";
            return await next(context);
        }
    }
}
        