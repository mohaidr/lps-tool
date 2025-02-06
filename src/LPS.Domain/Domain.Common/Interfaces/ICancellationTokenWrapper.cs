using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LPS.Domain.Common.Interfaces
{
    public interface ICancellationTokenWrapper
    {
        CancellationToken CancellationToken { get; }
    }
}
