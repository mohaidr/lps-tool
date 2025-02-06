using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Domain.Common.Enums
{
    //it is important to give the enums number based on their strength as this will be used to calculate the aggregate status
    // this design may change
    public enum ExecutionStatus
    {
        PendingExecution = 0,
        Scheduled = 1,
        Ongoing = 2,
        Completed = 3,
        Paused = 4,
        Cancelled = 5,
        Failed = 6,
        Unkown = -7,
    }
}
