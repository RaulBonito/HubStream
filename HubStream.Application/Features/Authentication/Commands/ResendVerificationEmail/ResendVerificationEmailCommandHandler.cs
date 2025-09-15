using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Services.Common; // Asegúrate de tener el using correcto

namespace HubStream.Application.Features.Authentication.Commands.ResendVerificationEmail
{
    public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public ResendVerificationEmailCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<Result> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Por seguridad, no reveles que el usuario no existe.
                return Result.Success();
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                // El email ya está confirmado, no hay nada que hacer.
                return Result.Success();
            }

            // Genera el token de confirmación.
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Envía el email de confirmación usando el servicio.
            await _emailService.SendConfirmationEmailAsync(user.Email, user.Id.ToString(), token);

            return Result.Success();
        }
    }
}