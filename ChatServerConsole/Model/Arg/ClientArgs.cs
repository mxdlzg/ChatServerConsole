using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ChatServerConsole.Model
{
    public class ClientArgs
    {
        public const int ArgSuccess = 0;
        public const int ArgError = -1;

        public Event_Type EventType { get; set; }
        public int ErrorCode { get; set; }

        public ClientArgs()
        {
            ErrorCode = ArgSuccess;
        }

        public static T AnalysisBody<T>(dynamic body)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            if (body is string)
            {
                T dc = jss.Deserialize<T>(body);
                return dc;
            }
            else
            {
                var tmp = jss.Serialize(body);
                return jss.Deserialize<T>(tmp);
            }
        }

        public static string ToBody(ClientArgs clientArgs)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Serialize(clientArgs);
        }
    }
    
}
