using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace HubStream.Infrastructure.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // El ensamblado donde se encuentra esta clase (HubStream.Infrastructure)
            var assembly = Assembly.GetExecutingAssembly();

            // Busca todas las clases que no sean abstractas en el ensamblado
            var serviceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in serviceTypes)
            {
                // Para cada clase, busca todas las interfaces que implementa
                // y que no sean las interfaces genéricas de .NET (como IEnumerable, etc.)
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsPublic && !i.IsGenericType);

                foreach (var @interface in interfaces)
                {
                    // Registra el servicio con su interfaz.
                    // Ejemplo: services.AddScoped<IEmailService, EmailService>();
                    // Esto se hará para cada par <interfaz, clase> que encuentre.
                    services.AddScoped(@interface, type);
                }
            }

            // Si tuvieras algún servicio que no sigue la convención o requiere
            // un registro especial (como Singleton), puedes añadirlo manualmente aquí.
            // Ejemplo: services.AddSingleton<ISpecialService, SpecialService>();

            return services;
        }
    }
}