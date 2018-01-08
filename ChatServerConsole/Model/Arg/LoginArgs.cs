using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerConsole.Model.Arg
{
    public class LoginArgs:ClientArgs
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string UserId { get; set; }

        //res
        public bool Success { get; set; }

        public LoginArgs()
        {

        }

        public LoginArgs(string name, string password, string userId)
        {
            Name = name;
            Password = password;
            UserId = userId;
        }
    }
}
