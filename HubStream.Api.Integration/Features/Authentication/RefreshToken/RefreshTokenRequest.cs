using HubStream.Api.Integration.Features.Authentication.Login;
using HubStream.Shared.Kernel.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Api.Integration.Features.Authentication.RefreshToken
{
    public class RefreshTokenRequest
    {
        // El access token expirado, para obtener el userId de forma segura
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
