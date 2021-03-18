using System;
using System.Linq;

namespace Blogifier.Shared
{
    public record SystemLog
    {
        public int Id;
        public string Message;
        public string MessageTemplate;
        public string Level;
        public DateTime TimeStamp;
        public string Exception;
        public string Properties;
    }
}
