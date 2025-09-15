using HubStream.Api.Integration.Features.Authentication.Login;
using HubStream.Shared.Kernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Api.Integration.Features.Authentication.ExternalLogin
{
    public class ExternalLoginRequest 
    {
        public string Provider { get; set; } // "Google", "Facebook", etc.
        public string IdToken { get; set; }
    }
}
