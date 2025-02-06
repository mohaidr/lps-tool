using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.CachService
{
    public static class CachePrefixes
    {
        public const string Content = "Content_";
        public const string ResourceUrls = "ResourceUrls_";
        public const string Multipartfile = "multipartfile_";
        public const string SampleResponse = "SampleResponse_";
        public const string RequestSize = "request_size_";
        public const string GlobalCounter = "GlobalCounter_";
        public const string SessionCounter = "Counter_";
        public const string Path = "Path_";
        public const string Placeholder = "Placeholder_";
    }
}
