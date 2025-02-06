using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Domain.Common.Enums
{
    public enum IterationMode
    {
        /* 
        * D refers to Duration
        * C refers to Cool Down
        * B refers to Batch Size  
        * R refers to Request Count
        */
        Default = 0, // This is currently used as a workaround (hack) because the custom JSON serializer is configured to ignore default values. Since 0 is the default value, this ensures that DCB is not ignored. However, this approach violates our coding standards.
        DCB = 1,
        CRB = 2,
        CB = 3,
        R = 4,
        D = 5
    }
}
