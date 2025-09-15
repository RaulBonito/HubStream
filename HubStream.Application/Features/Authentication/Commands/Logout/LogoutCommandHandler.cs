using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LogoutCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                // No devuelvas un error, ya que el objetivo es que el usuario quede deslogueado.
                return Result.Success();
            }

            // Invalida el refresh token
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return Result.Success();
        }
    }
}
