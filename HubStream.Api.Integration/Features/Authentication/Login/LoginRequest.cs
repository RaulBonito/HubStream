using HubStream.Shared.Kernel.Common;


namespace HubStream.Api.Integration.Features.Authentication.Login
{
    public class LoginRequest 
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
