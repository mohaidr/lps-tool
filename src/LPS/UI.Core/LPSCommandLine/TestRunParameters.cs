using LPS.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.UI.Core.LPSCommandLine
{
    public record TestRunParameters
    {
        public bool IsInline { get; }
        public TestRunParameters(PlanDto planDto, CancellationToken token)
        {
            IsInline = true;
            PlanDto = planDto;
            RoundNames = [];
            Tags = [];
            Environments = [];
            CancellationToken = token;

        }
        public TestRunParameters(string configFile, IList<string> roundNames, IList<string> tags, IList<string> environments, CancellationToken token)
        {
            IsInline = false;
            ConfigFile = configFile;
            RoundNames = roundNames;
            Tags = tags;
            Environments = environments;
            CancellationToken = token;
        }

        public string ConfigFile { get; private set; }
        public PlanDto PlanDto { get; private set; }
        public IList<string> RoundNames { get; private set; }
        public IList<string> Tags { get; private set; }
        public IList<string> Environments { get; private set; }

        public CancellationToken CancellationToken { get; private set; }
    }
}
