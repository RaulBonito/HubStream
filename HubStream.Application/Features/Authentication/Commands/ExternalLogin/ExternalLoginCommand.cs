using HubStream.Application.Features.Authentication.Commands.Login;
using HubStream.Shared.Kernel.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.ExternalLogin
{
    public class ExternalLoginCommand : IRequest<Result<LoginResult>>
    {
        public string Provider { get; set; } // "Google", "Facebook", etc.
        public string Code { get; set; }
        public string RedirectUri { get; set; }
    }
}
