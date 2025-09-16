using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyEmailCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Token))
            {
                return Result.Failure(new Error("Auth.InvalidVerificationRequest", "Faltan el usuario o el token."));
            }

            var userIdentifier = new Identifier(request.UserId);

            var user = _userManager.Users.FirstOrDefault(u=> u.Id == userIdentifier);
            if (user == null)
            {
                return Result.Failure(new Error("Auth.UserNotFound", "Usuario no encontrado."));
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure(new Error("Auth.EmailVerificationFailed", $"No se pudo verificar el email: {errors}"));
            }

            return Result.Success();
        }
    }
}
