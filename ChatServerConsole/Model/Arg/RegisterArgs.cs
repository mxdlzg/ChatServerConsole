using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerConsole.Model
{
    public class RegisterArgs : ClientArgs
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Sex { get; set; }
        public string City { get; set; }
        public string UserType { get; set; }

        public string User_ID { get; set; }
    }
}
