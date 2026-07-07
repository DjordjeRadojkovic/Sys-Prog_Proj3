using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_3.Messages
{
    public sealed class LogMessage
    {
        public string Text { get; }
        public int SenderThreadId { get; }
        public DateTime Time { get; }

        public LogMessage(string text, int senderThreadId)
        {
            Text = text;
            SenderThreadId = senderThreadId;
            Time = DateTime.Now;
        }
    }
}
