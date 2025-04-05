using LPS.UI.Core.LPSCommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.UI.Common
{
    public interface ITestExecutionService
    {
        Task ExecuteAsync(TestRunParameters parameters);
    }
}
