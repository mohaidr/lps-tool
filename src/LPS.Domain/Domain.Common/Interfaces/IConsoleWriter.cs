using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Domain.Common.Interfaces
{
    public interface IConsoleWriter
    {
        public int MaxNumberOfMessagesToDisplay { get; set; }
        public int MaxNumberOfMessages { get; set; }
        public void AddMessage(string message, int priority, int groupId, ConsoleColor color);
        public void StartWriting();
        public void StopWriting();
        public void ClearConsole();
    }
}
