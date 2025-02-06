using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Common.LPSSerializer
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class JsonAliasAttribute : Attribute
    {
        public string Alias { get; }

        public JsonAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }

}
