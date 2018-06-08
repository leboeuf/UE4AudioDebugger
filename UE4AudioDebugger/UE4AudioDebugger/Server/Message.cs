using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UE4AudioDebugger.Server
{
    public class Message
    {
        public DateTime Timestamp { get; internal set; }
        public byte[] MessageBytes { get; internal set; }
    }
}
