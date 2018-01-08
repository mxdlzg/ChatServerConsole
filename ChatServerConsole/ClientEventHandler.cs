using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatServerConsole.DB;
using ChatServerConsole.Model;
using ChatServerConsole.Model.Arg;

namespace ChatServerConsole
{
    public class ClientEventHandler
    {
        private ClientEvent currentEvent;

        /// <summary>
        /// 分发事件，此时仍然停留在read的线程
        /// </summary>
        /// <param name="eEvent"></param>
        /// <returns></returns>
        public string DespatchEvent(ClientEvent eEvent)
        {
            if (eEvent == null)
            {
                return "null despatch";
            }
            this.currentEvent = eEvent;
            switch (eEvent.Type)
            {
                case Event_Type.Register:
                    ExeRegister(eEvent);
                    break;
                case Event_Type.Login:
                    ExeLoginEvent(eEvent);
                    break;
                case Event_Type.Logout:
                    ExeLogoutEvent(eEvent);
                    break;
                case Event_Type.SendMessage:
                    ExeSendMessageEvent(eEvent);
                    break;
                case Event_Type.PublishMessage:
                    ExePublishMessage(eEvent);
                    break;
                default:
                    Default(eEvent);break;
            }

            return "despatch";
        }

        private void ExeRegister(ClientEvent eEvent)
        {
            //Log
            Log.Info("thread" + Thread.CurrentThread.ManagedThreadId + " exe reigister event", eEvent.Client.LogSource);

            //OP
            RegisterArgs args = ClientArgs.AnalysisBody<RegisterArgs>(eEvent.Body);
            DbResult<string> result = DbUserAction.Register(args.Name,args.Password,args.Sex,args.City,args.UserType);
            if (result.Status == DbEnum.Success)
            {
                eEvent.Client.Send(new ServerEvent() {Type = Event_Type.Register,RawContent = ClientArgs.ToBody(new RegisterArgs() {User_ID = result.Data,ErrorCode = ClientArgs.ArgSuccess}),SendTime = DateTime.Now.ToString()}.ToString());
            }
            else
            {
                eEvent.Client.Send(new ServerEvent() {Type = Event_Type.Register,RawContent = ClientArgs.ToBody(new BadRequestArgs() {Code = result.ErrorCode,Message = result.Error}),SendTime = DateTime.Now.ToString()}.ToString());
            }
        }

        private void ExePublishMessage(ClientEvent eEvent)
        {
            Console.WriteLine("thread" + Thread.CurrentThread.ManagedThreadId + " despatch publish event");
        }

        private void ExeSendMessageEvent(ClientEvent eEvent)
        {
            Console.WriteLine("thread" + Thread.CurrentThread.ManagedThreadId + " despatch send event");
        }

        /// <summary>
        /// 退出登录状态，但是不断开socket连接，如果需要断开连接，客户端发送'exit<EOF>'即可
        /// </summary>
        /// <param name="eEvent"></param>
        private void ExeLogoutEvent(ClientEvent eEvent)
        {
            //Log
            Log.Info("thread" + Thread.CurrentThread.ManagedThreadId + " exe logout event", eEvent.Client.LogSource);

            //OP
            C_User cUser = ClientArgs.AnalysisBody<C_User>(eEvent.Body);

            //DB OP
            var result = DbUserAction.Logout(cUser.User_ID, eEvent.Client.Address.ToString(),eEvent.Client.Port);
            if (result.Status == DbEnum.Success)
            {
                //Log
                Log.Warn("update as Offline",eEvent.Client.LogSource);

                //status
                lock (eEvent.Client)
                {
                    eEvent.Client.Login = false;
                }

                //Send Back
                if (eEvent.Client.Socket.Connected)
                {
                    eEvent.Client.Send(NewEvent(Event_Type.Logout, result.Data).ToString());
                }
            }
            else
            {
                //Log
                Log.Warn(result.Error,eEvent.Client.LogSource);

                //Send Error Back 
                eEvent.Client.Send(NewEvent(Event_Type.Logout,new BadRequestArgs() {Code = result.ErrorCode,Message = result.Error}).ToString());
            }


        }

        private void ExeLoginEvent(ClientEvent eEvent)
        {
            //Log
            Log.Info("thread" + Thread.CurrentThread.ManagedThreadId + " exe login event", eEvent.Client.LogSource);

            //OP
            LoginArgs loginArgs = ClientArgs.AnalysisBody<LoginArgs>(eEvent.Body);

            //DB OP
            var result = DbUserAction.Login(loginArgs.Name, loginArgs.Password,eEvent.Client.Address.ToString(),eEvent.Client.Port);
            if (result.Data.Success)
            {
                lock (eEvent.Client)
                {
                    eEvent.Client.Login = true;
                }

                eEvent.Client.Send(new ServerEvent() {Type = Event_Type.Login,RawContent = ClientArgs.ToBody(result.Data),SendTime = DateTime.Now.ToString()}.ToString());
            }
            else
            {
                lock (eEvent.Client)
                {
                    eEvent.Client.Login = false;
                }
                eEvent.Client.Send(new ServerEvent() {Type = Event_Type.Login,RawContent = ClientArgs.ToBody(new BadRequestArgs() {Code = result.ErrorCode,Message = result.Error}),SendTime = DateTime.Now.ToString()}.ToString());
            }
        }


        private void Default(ClientEvent eEvent)
        {
            throw new ArgumentOutOfRangeException();
        }

        private ServerEvent NewEvent(Event_Type type, ClientArgs args)
        {
            return new ServerEvent() {Type = type,RawContent = ClientArgs.ToBody(args),SendTime = DateTime.Now.ToString()};
        }

    }

}
