using HubStream.Shared.Kernel.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.VerifyEmail
{
    public class VerifyEmailCommand : IRequest<Result>
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}
