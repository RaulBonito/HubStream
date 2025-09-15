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
    public class SignUpCommand : IRequest<Result<SignUpResult>>
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
