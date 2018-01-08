using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerConsole.Model
{
    public enum DbEnum
    {
        Success,
        Fail,
        Error
    }


    public class DbResult<T>
    {
        public static int LOGOUT_ERROR = -12;
        public const int SUCCESS = 21;
        public const int EXIST = -10;
        public const int USER_NOT_FOUND = -11;
        public const int LOGIN_SUCCESS = 20;
        public const string EXIST_ERROR = "已存在";

        public T Data { get; set; }
        public DbEnum Status { get; set; }
        public int ErrorCode { get; set; }
        public string Error { get; set; }

        public DbResult(T data)
        {
            this.Data = data;
            this.Status = DbEnum.Success;
        }

        public DbResult(T data,DbEnum status)
        {
            this.Data = data;
            this.Status = status;
        }

        public DbResult(T data, int errorCode)
        {
            this.Data = data;
            this.Status = DbEnum.Error;
            this.ErrorCode = errorCode;
            this.Error = "error";
        }

        public DbResult(T data, int errorCode,string error)
        {
            this.Data = data;
            this.Status = DbEnum.Error;
            this.ErrorCode = errorCode;
            this.Error = error;
        }
    }
}
