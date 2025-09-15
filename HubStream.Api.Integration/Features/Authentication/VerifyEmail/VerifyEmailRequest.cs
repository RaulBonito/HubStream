using HubStream.Shared.Kernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Api.Integration.Features.Authentication.VerifyEmail
{
    public class VerifyEmailRequest
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}
