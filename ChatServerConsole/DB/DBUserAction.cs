using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ChatServerConsole.Model;
using ChatServerConsole.Model.Arg;

namespace ChatServerConsole.DB
{
    public class DbUserAction:DbAction
    {
        public DbUserAction(ChatDBEntities dbContext) : base(dbContext)
        {
        }

        public static DbResult<string> Register(string name, string password, string sex, string city, string userType)
        {
            ChatDBEntities db = new ChatDBEntities();
            UserRegister_Result result = db.UserRegister(name, password, sex, city, userType,
                new ObjectParameter("userID", typeof(string))).First();
            if (result.ERROR_CODE == Config.ERROR_CODE_NONE)
            {
                return new DbResult<string>(result.User_ID,DbEnum.Success);
            }
            else
            {
                return new DbResult<string>("",DbResult<string>.EXIST,"此用户已存在！");
            }
        }

        public static DbResult<LoginArgs> Login(string name, string password, string ip, int port)
        {
            ChatDBEntities db = new ChatDBEntities();
            DbResult<LoginArgs> dbResult = null;
            int efResult = (int) db.Login(name, password, ip, port).First();
            if (efResult == DbResult<bool>.LOGIN_SUCCESS)
            {
                var user = db.C_User.FirstOrDefault(u => u.User_Name == name);
                dbResult = new DbResult<LoginArgs>(new LoginArgs() {UserId = user?.User_ID,Success = true});
            }
            else
            {
                dbResult = new DbResult<LoginArgs>(new LoginArgs(),efResult,"登录失败,状态:"+efResult);
            }
            return dbResult;
        }

        public static DbResult<LogoutArgs> Logout(string userId, string ip, int port)
        {
            ChatDBEntities db = new ChatDBEntities();
            db = new ChatDBEntities();
            DbResult<LogoutArgs> dbResult = null;
            var cus = db.C_User_Status.FirstOrDefault(cu => cu.IP==ip && cu.Port==port && cu.User_ID==userId);
            if (cus != null)
            {
                cus.Status_ID = "002";
                var efState = db.Entry(cus).State = EntityState.Modified;
                var efResult = db.SaveChanges();
                if (efResult > 0)
                {
                    dbResult = new DbResult<LogoutArgs>(new LogoutArgs() { ErrorCode = ClientArgs.ArgSuccess });
                }
            }
            else
            {
                dbResult = new DbResult<LogoutArgs>(new LogoutArgs() { ErrorCode = ClientArgs.ArgError },DbResult<LogoutArgs>.LOGOUT_ERROR,"未找到对应状态");
            }


            return dbResult;
        }

        public static int LogoutIp(string address, int port)
        {
            ChatDBEntities db = new ChatDBEntities();
            var cus = db.C_User_Status.FirstOrDefault(cu => cu.IP==address && cu.Port==port);
            cus.Status_ID = "002";
            db.Entry(cus).State = EntityState.Modified;
            var efResult = db.SaveChanges();
            return efResult;
        }
    }
}
