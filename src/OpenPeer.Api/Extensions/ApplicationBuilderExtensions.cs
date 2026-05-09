using OpenPeer.Api.Middleware;

namespace OpenPeer.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseOpenPeerMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }
}
