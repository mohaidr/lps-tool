using System;
using System.Text.Json.Serialization;

namespace LPS.Infrastructure.Common.Interfaces
{

    public interface IHttpDimensionSet : IDimensionSet
    {
        public string RoundName { get;}
        public Guid IterationId { get; }
        public string IterationName { get; }
        public string URL { get;}
        public string HttpMethod { get;}
        public string HttpVersion { get;}
    }
}
