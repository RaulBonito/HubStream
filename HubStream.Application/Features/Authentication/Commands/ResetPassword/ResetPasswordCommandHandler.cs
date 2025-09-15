using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return Result.Failure(new Error("Auth.PasswordMismatch", "Las contraseñas no coinciden."));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Mensaje de error genérico para no revelar información
                return Result.Failure(new Error("Auth.ResetFailed", "La operación para restablecer la contraseña ha fallado."));
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure(new Error("Auth.ResetFailed", $"No se pudo restablecer la contraseña: {errors}"));
            }

            return Result.Success();
        }
    }
}
