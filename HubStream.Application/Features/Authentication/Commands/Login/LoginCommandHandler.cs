using HubStream.Application.Services.Common;
using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result<LoginResult>.Failure(new Error("Auth.InvalidCredentials", "El email o la contraseña son incorrectos."));

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
                return Result<LoginResult>.Failure(new Error("Auth.InvalidCredentials", "El email o la contraseña son incorrectos."));


            var token = await _jwtTokenGenerator.GenerateTokenAsync(user);
            var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user);

            var loginResult = new LoginResult
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                Token = token,
                RefreshToken = refreshToken
            };

            return Result<LoginResult>.Success(loginResult);
        }
    }

}
