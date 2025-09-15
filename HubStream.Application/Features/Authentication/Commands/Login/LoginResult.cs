using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Features.Authentication.Commands.Login
{
    public class LoginResult
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; } // Suponiendo que usaremos JWT para la autenticación
        public string RefreshToken { get; set; } // Suponiendo que usaremos JWT para la autenticación
    }
}
