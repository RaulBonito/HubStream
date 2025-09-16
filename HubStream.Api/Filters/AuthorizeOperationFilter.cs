using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HubStream.Api.Filters
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Verifica si el endpoint tiene el atributo [AllowAnonymous]
            var allowAnonymous = context.MethodInfo.GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any();

            if (allowAnonymous)
            {
                return; // No aplicar seguridad a endpoints con [AllowAnonymous]
            }

            // Verifica si el controlador o el método tienen el atributo [Authorize]
            var hasAuthorize = context.MethodInfo.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any() ||
                context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any();

            if (hasAuthorize)
            {
                // Añade el requerimiento de seguridad solo a endpoints autorizados
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    }
                };

                // Añade posibles respuestas de error de autenticación
                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
            }
        }
    }
}