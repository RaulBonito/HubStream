using Google.Apis.Auth;
using HubStream.Application.Features.Authentication.Commands.Login;
using HubStream.Application.Services.Common;
using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
// Asegúrate de tener las bibliotecas adecuadas para Microsoft si usas validación de token a nivel de backend.
// Para Microsoft, a menudo se usa un flujo OAuth2/OpenID Connect con validación de tokens JWT en el cliente o directamente con Identity.

namespace HubStream.Application.Features.Authentication.Commands.ExternalLogin
{
    public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, Result<LoginResult>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IConfiguration _configuration;

        public ExternalLoginCommandHandler(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator, IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _configuration = configuration;
        }

        public async Task<Result<LoginResult>> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string email = null;
                string subject = null; // En Google es payload.Subject, en otros podría ser el ID de usuario del proveedor.

                // 1. Validar el token externo y extraer información del usuario
                if (request.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
                {
                    var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                    };
                    var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);
                    email = payload.Email;
                    subject = payload.Subject;
                }
                else if (request.Provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))
                {
                    // Para Microsoft, la validación del token JWT puede ser similar a Google si estás recibiendo un ID Token.
                    // Sin embargo, si estás utilizando un flujo de autenticación estándar de ASP.NET Core Identity con proveedores externos,
                    // la validación del token de acceso se realiza a menudo en el cliente, y el backend recibe un ClaimsPrincipal.
                    // Si recibes un ID Token de Microsoft, puedes validarlo usando una librería JWT o implementando la validación.
                    // Para este ejemplo, asumiremos que recibes un ID Token que puedes validar.
                    // **NOTA:** La validación de tokens de Microsoft es más compleja y a menudo requiere una librería como
                    // Microsoft.IdentityModel.Tokens o directamente usar el middleware de autenticación de ASP.NET Core
                    // para external logins, que ya maneja la validación y te proporciona los claims.
                    // Si estás enviando el 'IdToken' desde el frontend, aquí tendrías que validarlo.
                    // Por ejemplo, usando una librería JWT genérica o una específica para Microsoft.

                    // EJEMPLO ILUSTRATIVO (NO COMPLETO PARA PRODUCCIÓN SIN LIBRERÍA DE VALIDACIÓN JWT):
                    // var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    // var jsonToken = handler.ReadToken(request.IdToken) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                    // email = jsonToken?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                    // subject = jsonToken?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value; // O 'oid' para object ID de Azure AD

                    // ALTERNATIVA: Si ya usas el middleware de autenticación de ASP.NET Core Identity para Microsoft,
                    // el flujo sería diferente; este handler se enfocaría en asociar el login externo ya autenticado.
                    // Para este escenario de "recibir IdToken y validarlo en el backend", necesitarías una librería JWT robusta
                    // para validar tokens de Microsoft y extraer los claims.

                    // Por simplicidad para este ejemplo, vamos a asumir que podemos obtener el email y un identificador único
                    // de alguna manera después de una "validación" (o que el cliente nos lo envía ya validado y confiable).
                    // En un escenario real, usarías Microsoft.Identity.Web o alguna librería similar para esto.
                    throw new NotImplementedException("La validación de tokens de Microsoft requiere una implementación más robusta. Considera usar el middleware de autenticación de ASP.NET Core Identity para proveedores externos.");
                }
                else
                {
                    return Result<LoginResult>.Failure(new Error("Auth.UnsupportedProvider", $"Proveedor externo no soportado: {request.Provider}"));
                }

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(subject))
                {
                    return Result<LoginResult>.Failure(new Error("Auth.InvalidExternalToken", "No se pudo extraer el email o el identificador del token externo."));
                }

                // 2. Buscar si el usuario ya existe con este login externo
                var info = new UserLoginInfo(request.Provider, subject, request.Provider);
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                if (user == null)
                {
                    // Si no existe, buscar por email
                    user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        // Si no existe ni por login ni por email, es un usuario nuevo
                        user = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true // El proveedor ya lo ha verificado
                        };
                        var createResult = await _userManager.CreateAsync(user);
                        if (!createResult.Succeeded)
                        {
                            return Result<LoginResult>.Failure(new Error("User.CreationFailed", "No se pudo crear el usuario."));
                        }
                    }

                    // Añadir el login externo a la cuenta del usuario (nueva o existente)
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        return Result<LoginResult>.Failure(new Error("Login.AddLoginFailed", "No se pudo añadir el login externo al usuario."));
                    }
                }

                // 3. Generar los tokens de nuestra aplicación
                var accessToken = await _jwtTokenGenerator.GenerateTokenAsync(user);
                var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user);

                var loginResult = new LoginResult
                {
                    UserId = user.Id.ToString(),
                    Email = user.Email,
                    Token = accessToken,
                    RefreshToken = refreshToken
                };

                return Result<LoginResult>.Success(loginResult);
            }
            catch (InvalidJwtException ex)
            {
                return Result<LoginResult>.Failure(new Error("Auth.InvalidExternalToken", $"Token externo inválido para Google: {ex.Message}"));
            }
            catch (NotImplementedException ex) // Capturar la excepción específica de Microsoft si se lanza.
            {
                return Result<LoginResult>.Failure(new Error("Auth.ProviderConfigurationError", $"Error de configuración o implementación para el proveedor: {ex.Message}"));
            }
            catch (Exception ex)
            {
                // Aquí puedes loggear el error para debugging.
                return Result<LoginResult>.Failure(new Error("Auth.ExternalLoginError", $"Ocurrió un error inesperado durante el inicio de sesión externo: {ex.Message}"));
            }
        }
    }
}