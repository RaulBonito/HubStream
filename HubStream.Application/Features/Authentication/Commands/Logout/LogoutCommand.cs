using HubStream.Shared.Kernel.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.Logout
{
    public class LogoutCommand : IRequest<Result>
    {
        public string UserId { get; set; }
    }
}
