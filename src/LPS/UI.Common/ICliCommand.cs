using LPS.Domain.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading;

namespace LPS.UI.Common
{
    internal interface ICliCommand
    {
        Command Command { get; }
        void SetHandler(CancellationToken cancellationToken);
    }
}
