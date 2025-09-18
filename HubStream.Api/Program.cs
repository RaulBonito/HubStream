// Importaciones necesarias para el proyecto
using HubStream.Api.Filters;
using HubStream.Application.Features.Authentication.Commands.Login;
using HubStream.Application.Settings;
using HubStream.Infrastructure.Middlewares;
using HubStream.Infrastructure.Persistence;
using HubStream.Infrastructure.Persistence.Contexts;
using HubStream.Infrastructure.Persistence.Seeds;
using HubStream.Infrastructure.Services;
using HubStream.Shared.Kernel.Common;
using HubStream.Shared.Kernel.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
// Añadido para OpenApiInfo
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

#region Servicios
// -----------------------------------------------------------------------------
// Registro de servicios en el contenedor de dependencias de ASP.NET Core.
// -----------------------------------------------------------------------------


var jwtSettings = new JwtSettings();
configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

var appSettings = new AppSettings();
configuration.GetSection(AppSettings.SectionName).Bind(appSettings);
builder.Services.AddSingleton(appSettings);


// 1. Controladores de la API
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// 2. Configuración de Autenticación y Autorización
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            // Usa la propiedad de la clase en lugar de la magic string
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // --- INICIO DE LA MODIFICACIÓN ---
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Este evento se dispara si el token es inválido (expirado, mala firma, etc.)
                // Puedes registrar el error si lo deseas.
                // context.Exception contiene los detalles del fallo.
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Error de autenticación JWT.");

                // No es necesario escribir una respuesta aquí, OnChallenge se encargará.
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // Este evento se dispara cuando la autenticación falla y se va a enviar una respuesta 401.
                // Aquí personalizamos esa respuesta para que use ApiResponse.

                // Marca el evento como manejado para que no se ejecute la lógica por defecto.
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var error = new ApiError
                {
                    Code = "UNAUTHORIZED",
                    Message = "No estás autenticado o tu token es inválido."
                };

                // Si hubo un error específico (como token expirado), podemos añadir más detalles.
                if (context.AuthenticateFailure != null)
                {
                    error.Message = $"Error de autenticación: {context.AuthenticateFailure.Message}";
                }

                var response = ApiResponse<object>.CreateFailure(error);
                var jsonResponse = JsonSerializer.Serialize(response);

                return context.Response.WriteAsync(jsonResponse);
            }
        };
        // --- FIN DE LA MODIFICACIÓN ---
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
    });
builder.Services.AddAuthorization();

// 3. Soporte para OpenAPI/Swagger
// 3. Soporte para OpenAPI/Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HubStream API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa tu token JWT con el prefijo 'Bearer '. Ejemplo: 'Bearer {tu_token}'"
    });

    // ELIMINA el AddSecurityRequirement global y añade el filtro:
    c.OperationFilter<AuthorizeOperationFilter>();
});

// 4. SignalR
builder.Services.AddSignalR();

// 5. CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
    {
        policyBuilder
            .WithOrigins("https://localhost:7123", "http://localhost:5123")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 6. Registro de servicios de la capa de Infraestructura
builder.Services.AddInfrastructurePersistence(builder.Configuration);
builder.Services.AddInfrastructureServices();

// 7. Middleware personalizado de excepciones
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

// 8. Logging
builder.Services.AddLogging();

// 9. MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));

#endregion

var app = builder.Build();

#region Aplicar Migraciones y Seeding
// Aplicar migraciones al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var identityContext = services.GetRequiredService<AppIdentityDbContext>();
    identityContext.Database.Migrate();

    //var appContext = services.GetRequiredService<AppDbContext>();
    //appContext.Database.Migrate();
}

// Ejecutar el Seeder de datos
using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider;
    await IdentityDataSeeder.SeedAsync(provider, builder.Configuration);
}
#endregion

#region Middleware Pipeline
// -----------------------------------------------------------------------------
// Configuración de la tubería de middlewares de ASP.NET Core.
// -----------------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANTE: CORS debe ir antes de Authentication/Authorization.
app.UseCors("AllowSpecificOrigins");

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// IMPORTANTE: UseAuthentication() DEBE ir antes de UseAuthorization().
app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Endpoints
// -----------------------------------------------------------------------------
// Definición de endpoints expuestos por la aplicación.
// -----------------------------------------------------------------------------

app.MapControllers();
app.MapHub<SignalingHub>("/signalinghub");

#endregion

// Inicia la aplicación web.
app.Run();