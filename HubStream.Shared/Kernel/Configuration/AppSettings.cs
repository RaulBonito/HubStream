using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Shared.Kernel.Configuration
{
    public class AppSettings
    {
        public const string SectionName = "AppSettings";

        public string FrontendUrl { get; set; }
        public string FrontendVerificationUrl { get; set; }


    }
}
