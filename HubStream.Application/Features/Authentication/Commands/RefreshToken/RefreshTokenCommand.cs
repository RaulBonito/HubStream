using HubStream.Application.Features.Authentication.Commands.Login;
using HubStream.Shared.Kernel.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<Result<LoginResult>>
    {
        // El access token expirado, para obtener el userId de forma segura
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
