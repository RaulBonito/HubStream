using HubStream.Shared.Kernel.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Api.Integration.Features.Authentication.Logout
{
    public class LogoutRequest 
    {
        public string UserId { get; set; }
    }
}
