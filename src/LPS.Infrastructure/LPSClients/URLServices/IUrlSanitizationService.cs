using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.URLServices
{
    public interface IUrlSanitizationService
    {
        string Sanitize(string url);
    }
}
