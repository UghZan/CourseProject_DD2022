namespace API.Middleware
{
    using API.Exceptions;
    using Common.Extentions;
    using DAL.Entities;
    using System;

    namespace Api.Middlewares
    {
        public class ErrorMiddleware
        {
            private readonly RequestDelegate _next;

            public ErrorMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    await _next(context);
                }
                catch (NotFoundException ex)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsJsonAsync(ex.Message);
                }
                catch (ExistsException ex)
                {
                    context.Response.StatusCode = 409;
                    await context.Response.WriteAsJsonAsync(ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(ex.Message);
                }
                catch (InvalidException ex)
                {
                    context.Response.StatusCode = 406;
                    await context.Response.WriteAsJsonAsync(ex.Message);
                }
                catch (PermissionException ex)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(ex.Message);
                }

            }
        }
        public static class ErrorMiddlewareExtensions
        {
            public static IApplicationBuilder UseGlobalErrorWrapper(
                this IApplicationBuilder builder)
            {
                return builder.UseMiddleware<ErrorMiddleware>();
            }
        }
    }
}
