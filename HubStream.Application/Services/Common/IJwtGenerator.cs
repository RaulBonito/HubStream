using HubStream.Domain.Users.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Services.Common
{
    public interface IJwtTokenGenerator
    {
        Task<string> GenerateTokenAsync(ApplicationUser user);
        Task<string> GenerateRefreshTokenAsync(ApplicationUser user);
        Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
    }
}
