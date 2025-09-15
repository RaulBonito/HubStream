using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return Result.Failure(new Error("Auth.PasswordMismatch", "La nueva contraseña y su confirmación no coinciden."));
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Result.Failure(new Error("Auth.UserNotAuthenticated", "El usuario no está autenticado."));
            }

            var userIdentifier = new Identifier(userId);
            var user = _userManager.Users.FirstOrDefault(u=> u.Id == userIdentifier);
            if (user == null)
            {
                return Result.Failure(new Error("Auth.UserNotFound", "Usuario no encontrado."));
            }

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure(new Error("Auth.ChangePasswordFailed", $"No se pudo cambiar la contraseña: {errors}"));
            }

            return Result.Success();
        }
    }
}
