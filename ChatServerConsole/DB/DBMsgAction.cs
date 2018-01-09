using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatServerConsole.Model;
using ChatServerConsole.Model.Arg;

namespace ChatServerConsole.DB
{
    class DBMsgAction
    {
        public static DbResult<string> AddSingleMsg(C_Single_Msg cSingleMsg)
        {
            DbResult<string> dbResult = null;

            //db op
            using (ChatDBEntities db = new ChatDBEntities())
            {
                db.C_Single_Msg.Add(cSingleMsg);

                db.SaveChanges();

                if (cSingleMsg.ID != 0)
                {
                    dbResult = new DbResult<string>(cSingleMsg.ID.ToString());
                }

                //exe dbo proceduce
                //var result = db.AddSingleMsg(cSingleMsg.Msg_Content, cSingleMsg.Send_Time, cSingleMsg.From_User_ID,
                //    cSingleMsg.To_User_ID, "004").FirstOrDefault();

                ////check
                //if (result == DbResult<string>.SUCCESS)
                //{
                //    dbResult = new DbResult<string>(msg.Single_Msg_ID);
                //}
            }

            //result
            return dbResult ?? new DbResult<string>(null);
        }

        public static void UpdateMsg(C_Single_Msg cSingleMsg)
        {
            using (ChatDBEntities db =  new ChatDBEntities())
            {
                db.Entry(cSingleMsg).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static DbResult<PublishMsgArgs> AddMultiMsg(C_Multi_Msg cMultiMsg)
        {
            DbResult<PublishMsgArgs> dbResult = null;
            using (ChatDBEntities db = new ChatDBEntities())
            {
                db.C_Multi_Msg.Add(cMultiMsg);
                db.SaveChanges();
                if (cMultiMsg.ID!=0)
                {
                    dbResult = new DbResult<PublishMsgArgs>(new PublishMsgArgs() {ErrorCode = DbResult<PublishMsgArgs>.SUCCESS});
                }
            }

            return dbResult ?? new DbResult<PublishMsgArgs>(null);
        }
    }
}


//try
//{
//    db.SaveChanges();
//}
//catch (DbEntityValidationException e)
//{
//    //foreach (var eve in e.EntityValidationErrors)
//    //{
//    //    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
//    //        eve.Entry.Entity.GetType().Name, eve.Entry.State);
//    //    foreach (var ve in eve.ValidationErrors)
//    //    {
//    //        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
//    //            ve.PropertyName, ve.ErrorMessage);
//    //    }
//    //}
//    throw;
//}