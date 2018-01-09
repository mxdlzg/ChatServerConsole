using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatServerConsole.DB;
using ChatServerConsole.Model;

namespace ChatServerConsole
{
    class ServerEventHandler
    {
        /// <summary>
        /// 分发事件，此时仍然停留在send callback的线程
        /// </summary>
        /// <param name="eEvent"></param>
        /// <returns></returns>
        public static string DespatchEvent(ServerEvent eEvent)
        {
            if (eEvent == null)
            {
                return "null despatch";
            }

            switch (eEvent.CallbackType)
            {
                case Event_Type.SendMsgCallback:
                    ExeSendMsgCallback(eEvent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return "despatch";
        }

        private static void ExeSendMsgCallback(ServerEvent eEvent)
        {
            C_Single_Msg cSingleMsg = (C_Single_Msg) eEvent.CallbackState;
            if (eEvent.CallbackSuccess)
            {
                cSingleMsg.Msg_Type_ID = "003";
                DBMsgAction.UpdateMsg(cSingleMsg);
            }
        }
    }
}
