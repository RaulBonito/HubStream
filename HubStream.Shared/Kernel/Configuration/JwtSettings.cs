using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Application.Settings
{
    // En una nueva carpeta, por ejemplo, Shared/Kernel/Configurations
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Key { get; init; } = null!;
        public string Issuer { get; init; } = null!;
        public string Audience { get; init; } = null!;
        public int ExpiryMinutes { get; init; }
        public int RefreshTokenExpiryDays { get; init; }
    }
}
