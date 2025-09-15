using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Users.Entities
{
    // Roles
    public class ApplicationRole : IdentityRole<Identifier> { }
    public class ApplicationRoleClaim : IdentityRoleClaim<Identifier> { }

    // Claims / Roles / Logins / Tokens para usuarios
    public class ApplicationUserClaim : IdentityUserClaim<Identifier> { }
    public class ApplicationUserRole : IdentityUserRole<Identifier> { }
    public class ApplicationUserLogin : IdentityUserLogin<Identifier> { }
    public class ApplicationUserToken : IdentityUserToken<Identifier> { }

}
