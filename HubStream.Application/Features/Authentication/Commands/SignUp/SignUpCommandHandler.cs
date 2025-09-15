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

namespace HubStream.Application.Features.Authentication.Commands.SignUp
{
    public class SignUpCommandHandler : IRequestHandler<SignUpCommand, Result<SignUpResult>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public SignUpCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<Result<SignUpResult>> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            // 1. Verificar si el usuario ya existe
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<SignUpResult>.Failure(new Error("User.EmailInUse", "El email proporcionado ya está en uso."));
            }

            // 2. Crear una nueva instancia de ApplicationUser
            var user = new ApplicationUser
            {
                Id = Identifier.New(),
                FullName = request.FullName,
                UserName = request.Email, // Identity por defecto usa UserName para el login
                Email = request.Email,
            };

            // 3. Usar UserManager para crear el usuario.
            var identityResult = await _userManager.CreateAsync(user, request.Password);

            // 4. Comprobar si la creación del usuario tuvo éxito
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return Result<SignUpResult>.Failure(new Error("User.CreationFailed", $"No se pudo crear el usuario: {errors}"));
            }

            // 5. Generar token de confirmación y enviar email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendConfirmationEmailAsync(user.Email, user.Id.ToString(), token);

            // 6. Crear el objeto de respuesta
            var signUpResult = new SignUpResult
            {
                UserId = user.Id.Id
            };

            // 7. Devolver un resultado exitoso
            return Result<SignUpResult>.Success(signUpResult);
        }
    }
}