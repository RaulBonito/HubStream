using HubStream.Application.Services.Common;
using HubStream.Domain.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HubStream.Infrastructure.Persistence.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtTokenGenerator(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _userManager = userManager;
        }

        public async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresInMinutes = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Genera un token de refresco criptográficamente seguro,
        /// lo guarda en la base de datos para el usuario y establece su fecha de expiración.
        /// </summary>
        public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user)
        {
            // 1. Generar un número aleatorio seguro
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            var refreshToken = Convert.ToBase64String(randomNumber);

            // 2. Asignar el token de refresco y su expiración al usuario
            user.RefreshToken = refreshToken;

            var expiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(expiryDays);

            // 3. Actualizar el usuario en la base de datos
            await _userManager.UpdateAsync(user);

            return refreshToken;
        }

        /// <summary>
        /// Valida un token JWT (incluso si ha expirado) y extrae su ClaimsPrincipal.
        /// Esto es necesario para el flujo de refresco, para identificar de forma segura al usuario
        /// a partir de un token de acceso ya expirado.
        /// </summary>
        public Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                // Aquí está la magia: no validamos la vida útil del token.
                ValidateLifetime = false,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                // Opcional: tolerancia de tiempo cero si no quieres margen de error
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // 1. Validar el token con los parámetros definidos
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                // 2. Comprobar que el token validado es realmente un JWT y usa el algoritmo esperado
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Si el token no es válido o el algoritmo no coincide, lanzamos una excepción
                    throw new SecurityTokenException("El token proporcionado no es válido.");
                }

                // 3. Devolver el principal con los claims del usuario
                return Task.FromResult(principal);
            }
            catch (Exception)
            {
                // Si la validación falla por cualquier motivo (firma incorrecta, formato inválido, etc.),
                // devolvemos null para indicar que el token no es de confianza.
                return Task.FromResult<ClaimsPrincipal>(null);
            }
        }
    }
}