using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerConsole.Model.Arg
{
    public class BadRequestArgs:ClientArgs
    {
        public string Message { get; set; }
        public int Code
        {
            get { return ErrorCode;}
            set { ErrorCode = value; }
        }
        public BadRequestArgs()
        {
            EventType = Event_Type.Bad;
            ErrorCode = 0;
        }
    }
}
