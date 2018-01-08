using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatServerConsole.Model;

namespace ChatServerConsole.DB
{
    public class DbAction
    {
        protected ChatDBEntities db;
        protected ChatDBEntities DB => db;

        public DbAction(ChatDBEntities dbContext)
        {
            this.db = dbContext;
        }
    }
}
