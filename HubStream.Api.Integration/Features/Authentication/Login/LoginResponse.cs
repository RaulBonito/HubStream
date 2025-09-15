using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Api.Integration.Features.Authentication.Login
{
    public class LoginResponse
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; } // Suponiendo que usaremos JWT para la autenticación
        public string RefreshToken { get; set; } // Suponiendo que usaremos JWT para la autenticación
    }
}
