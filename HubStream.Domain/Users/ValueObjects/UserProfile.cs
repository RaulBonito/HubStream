using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Users.ValueObjects
{
    public record UserProfile(string Bio, string BannerUrl, string ProfilePicUrl)
    {
        public static UserProfile CreateEmpty() => new(string.Empty, string.Empty, string.Empty);
    }
}
