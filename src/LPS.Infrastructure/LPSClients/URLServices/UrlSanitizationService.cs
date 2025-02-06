using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.URLServices
{
    public class UrlSanitizationService : IUrlSanitizationService
    {
        public string Sanitize(string url)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (var c in invalidChars)
            {
                url = url.Replace(c, '-');
            }
            return url;
        }
    }
}
