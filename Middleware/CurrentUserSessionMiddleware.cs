using Lab8_PhamVanTung_2324801030079.Constants;
using Lab8_PhamVanTung_2324801030079.Extensions;

namespace Lab8_PhamVanTung_2324801030079.Middleware;

public sealed class CurrentUserSessionMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && context.Session.GetObject<Models.Security.CurrentUser>(SessionConstants.CurrentUser) is null)
        {
            context.Session.SetObject(SessionConstants.CurrentUser, context.User.ToCurrentUser());
        }

        await _next(context);
    }
}
