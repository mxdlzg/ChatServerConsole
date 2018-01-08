using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChatServerConsole.DB;
using ChatServerConsole.Model;

namespace ChatServerConsole
{
    public class DBTest
    {
        private void test()
        {
            var db = new ChatDBEntities();

            foreach (var b in db.C_Msg_Type.Select(s => s.Msg_Type_Description == "11"))
            {
                Console.WriteLine(b);
            };
        }

        public static void register()
        {

        }
    }
}
