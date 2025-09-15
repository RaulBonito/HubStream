using HubStream.Shared.Kernel.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace HubStream.Infrastructure.Middlewares
{
    public class GlobalExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);

                context.Response.ContentType = "application/json";

                ApiError error;

                // Personaliza la respuesta según el tipo de excepción
                switch (ex)
                {
                    case UnauthorizedAccessException:
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        error = new ApiError
                        {
                            Code = "UNAUTHORIZED",
                            Message = "Invalid credentials or insufficient permissions."
                        };
                        break;

                    default:
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        error = new ApiError
                        {
                            Code = "INTERNAL_SERVER_ERROR",
                            Message = $"An unexpected error occurred. Please try again later. Details: {ex.Message}"
                        };
                        break;
                }

                var response = ApiResponse<object>.CreateFailure(error);
                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
            }
        }
    }
}
