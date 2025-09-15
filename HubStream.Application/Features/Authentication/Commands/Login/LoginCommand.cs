using HubStream.Shared.Kernel.Common;
using MediatR;

namespace HubStream.Application.Features.Authentication.Commands.Login
{
    public class LoginCommand : IRequest<Result<LoginResult>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
