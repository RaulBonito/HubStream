using HubStream.Application.Features.Authentication.Commands.Login;
using HubStream.Application.Services.Common;
using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResult>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RefreshTokenCommandHandler(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<LoginResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = await _jwtTokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                return Result<LoginResult>.Failure(new Error("Auth.InvalidToken", "El token de acceso es inválido."));
            }

            var userIdString = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null)
            {
                return Result<LoginResult>.Failure(new Error("Auth.InvalidToken", "No se pudo extraer el usuario del token."));
            }

            var identifier = Identifier.Parse(userIdString);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == identifier);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Result<LoginResult>.Failure(new Error("Auth.InvalidRefreshToken", "El token de refresco es inválido o ha expirado."));
            }

            var newAccessToken = await _jwtTokenGenerator.GenerateTokenAsync(user);
            // Opcional pero recomendado: rotar el refresh token
            var newRefreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user);

            var loginResult = new LoginResult
            {
                UserId = user.Id.ToString(),
                FullName = user.FullName,
                Email = user.Email,
                Token = newAccessToken,
                RefreshToken = newRefreshToken // Enviar el nuevo refresh token
            };

            return Result<LoginResult>.Success(loginResult);
        }
    }
}
