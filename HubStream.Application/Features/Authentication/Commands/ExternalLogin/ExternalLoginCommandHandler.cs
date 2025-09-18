using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.Services;
using HubStream.Application.Features.Authentication.Commands.Login;
using HubStream.Application.Services.Common;
using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.ExternalLogin
{
    public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, Result<LoginResult>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<ExternalLoginCommandHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ExternalLoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwtTokenGenerator,
            ILogger<ExternalLoginCommandHandler> logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<Result<LoginResult>> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string email;
                string subject;
                string fullName;

                // 1. Intercambiar el código por un token de acceso
                var tokenResponse = await ExchangeCodeForAccessTokenAsync(request.Code, request.RedirectUri);

                // 2. Validar el token de acceso y extraer información del usuario
                if (request.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Attempting to get user info using Google Access Token.");

                    // Usar el AccessToken para llamar a la API de Google People
                    var peopleService = new PeopleServiceService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = GoogleCredential.FromAccessToken(tokenResponse.AccessToken),
                        ApplicationName = "HubStream"
                    });

                    // Solicitar el perfil del usuario autenticado ('me')
                    var getRequest = peopleService.People.Get("people/me");
                    getRequest.PersonFields = "names,emailAddresses,metadata"; // Pedimos nombres, emails y metadatos (para el ID)

                    var person = await getRequest.ExecuteAsync(cancellationToken);

                    if (person == null)
                    {
                        return Result<LoginResult>.Failure(new Error("Auth.InvalidExternalToken", "No se pudo obtener la información del usuario desde Google."));
                    }

                    // Extraer email principal
                    email = person.EmailAddresses?.FirstOrDefault(e => e.Metadata?.Primary == true)?.Value;

                    // Extraer el ID de usuario (subject)
                    subject = person.Metadata?.Sources?.FirstOrDefault(s => s.Type == "PROFILE")?.Id;

                    // Extraer el nombre para mostrar
                    fullName = person.Names?.FirstOrDefault(n => n.Metadata?.Primary == true)?.DisplayName;

                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(subject))
                    {
                        return Result<LoginResult>.Failure(new Error("Auth.InvalidExternalToken", "No se pudo extraer el email o el identificador del token externo."));
                    }

                    _logger.LogInformation("Google user info retrieved successfully for email: {Email}", email);
                }
                else
                {
                    return Result<LoginResult>.Failure(new Error("Auth.UnsupportedProvider", $"Proveedor externo no soportado: {request.Provider}"));
                }

                // 3. Buscar si el usuario ya existe con este login externo
                var info = new UserLoginInfo(request.Provider, subject, request.Provider);
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                if (user == null)
                {
                    _logger.LogInformation("External login user not found for provider {Provider} and key {ProviderKey}. Searching by email {Email}", info.LoginProvider, info.ProviderKey, email);
                    user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        _logger.LogInformation("User not found by email. Creating a new user for {Email}", email);
                        user = new ApplicationUser
                        {
                            Id = Identifier.New(),
                            UserName = email, // O usa 'fullName' si lo prefieres
                            Email = email,
                            EmailConfirmed = true // El proveedor ya lo ha verificado
                        };
                        var createResult = await _userManager.CreateAsync(user);
                        if (!createResult.Succeeded)
                        {
                            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                            _logger.LogError("Failed to create new user: {Errors}", errors);
                            return Result<LoginResult>.Failure(new Error("User.CreationFailed", $"No se pudo crear el usuario: {errors}"));
                        }
                    }

                    _logger.LogInformation("Adding external login info to user {UserId}", user.Id);
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to add external login: {Errors}", errors);
                        return Result<LoginResult>.Failure(new Error("Login.AddLoginFailed", $"No se pudo añadir el login externo al usuario: {errors}"));
                    }
                }

                // 3. Generar los tokens de nuestra aplicación
                _logger.LogInformation("Generating internal tokens for user {UserId}", user.Id);
                var accessToken = await _jwtTokenGenerator.GenerateTokenAsync(user);
                var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user);

                var loginResult = new LoginResult
                {
                    UserId = user.Id.ToString(),
                    Email = user.Email,
                    FullName = user.UserName, // Opcionalmente, usa el 'fullName' obtenido de la API
                    Token = accessToken,
                    RefreshToken = refreshToken
                };

                return Result<LoginResult>.Success(loginResult);
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.LogError(ex, "Google API error. The access token may be invalid or expired.");
                return Result<LoginResult>.Failure(new Error("Auth.InvalidExternalToken", $"Token externo inválido para Google: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during external login.");
                return Result<LoginResult>.Failure(new Error("Auth.ExternalLoginError", $"Ocurrió un error inesperado durante el inicio de sesión externo: {ex.Message}"));
            }
        }

        private async Task<GoogleTokenResponse> ExchangeCodeForAccessTokenAsync(string code, string redirectUri)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var clientId = _configuration["Google:ClientId"];
            var clientSecret = _configuration["Google:ClientSecret"];

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            response.EnsureSuccessStatusCode();


            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GoogleTokenResponse>(responseString);
        }

        private class GoogleTokenResponse
        {
            public string AccessToken { get; set; }
            public string IdToken { get; set; }
            public string TokenType { get; set; }
            public int ExpiresIn { get; set; }
            public string RefreshToken { get; set; }
            public string Scope { get; set; }
        }
    }
}