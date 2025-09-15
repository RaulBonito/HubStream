using Application.Interfaces.Services.Common;
using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Por seguridad, no reveles que el usuario no existe.
                return Result.Success();
            }

            // Genera el token de reseteo de contraseña.
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Envía el email para restablecer la contraseña.
            // Nota: El `EmailService` ya se encarga de construir la URL completa.
            await _emailService.SendPasswordResetEmailAsync(request.Email, token);

            return Result.Success();
        }
    }
}