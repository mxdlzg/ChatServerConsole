using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            //Log
            Log.Info("thread" + Thread.CurrentThread.ManagedThreadId + " exe publishMessage event", eEvent.Client.LogSource);

            //Arg
            C_Multi_Msg cMultiMsg = ClientArgs.AnalysisBody<C_Multi_Msg>(eEvent.Body);

            //push msg into table
            DbResult<PublishMsgArgs> dbResult = DBMsgAction.AddMultiMsg(cMultiMsg);

            //Check DB status
            if (dbResult.Status == DbEnum.Success)
            {
                //Told Client,Server received
                eEvent.Client.Send(NewEvent(Event_Type.PublishMessage, new PublishMsgArgs() { ErrorCode = 0 }).ToString());
            }
            else
            {
                //Told Client,服务端收到，但是存储出现错误，将尝试进行分发（只有当前在线的人能收到此消息）
                eEvent.Client.Send(NewEvent(Event_Type.PublishMessage, new PublishMsgArgs() { ErrorCode = 0 }).ToString());
            }

            //get current online
            var addressList = DbUserAction.GetOnlineList(cMultiMsg.To_Group_ID);

            //Publish
            //var clientList = SocketListen.Clients.Where(c=>c.Address==)

            //DB OP
        }

        private void ExeSendMessageEvent(ClientEvent eEvent)
        {
            //Log
            Log.Info("thread" + Thread.CurrentThread.ManagedThreadId + " exe sendMessage event", eEvent.Client.LogSource);

            //Arg
            C_Single_Msg cSingleMsg = ClientArgs.AnalysisBody<C_Single_Msg>(eEvent.Body);
            cSingleMsg.Send_Time = eEvent.SendTime;

            //DB OP,将消息插入消息记录表，设置为Unsent状态,返回消息ID（Single_Msg_ID）
            DbResult<string> msgDbResult = DBMsgAction.AddSingleMsg(cSingleMsg);

            if (msgDbResult.Status == DbEnum.Success)
            {
                //Socket Op，告知客户端，服务器已收到消息（尚未进行消息分发）
                eEvent.Client.Send(NewEvent(Event_Type.SendMessage, new MsgArgs() { SingleMsgId = msgDbResult.Data }).ToString());
            }
            else
            {
                //Socket Op，告知客户端，服务器已收到消息（但是消息记录失败，将尝试分发，不保证消息可被保存）
                eEvent.Client.Send(NewEvent(Event_Type.SendMessage, new BadRequestArgs() {Code = msgDbResult.ErrorCode,Message = "消息记录失败，将尝试分发，不保证消息可被保存" }).ToString());
            }

            //DB OP,查找目标UserID的ip和port，如果在线则发送，否则消息设置为未读
            DbResult<C_User_Status> cUserStatus = DbUserAction.GetUserStatus(cSingleMsg.To_User_ID);

            //If online
            if (cUserStatus.Status == DbEnum.Success && cUserStatus.Data.Status_ID=="001")
            {
                //OP,查找目标client
                Client aimClient = SocketListen.FindClient(
                    new Tuple<IPAddress, int>(System.Net.IPAddress.Parse(cUserStatus.Data.IP),
                        (int)cUserStatus.Data.Port));

                //lock aim,send message
                if (aimClient != null)
                {
                    lock (aimClient)
                    {
                        if (aimClient.Login)
                        {
                            aimClient.Send(cSingleMsg.Msg_Content,new SendObject() {Client = aimClient, EventType = Event_Type.SendMsgCallback,RawContent = cSingleMsg}); //发送成功后，消息状态设置为Sent
                        }
                    }
                }
                else
                {
                    //目标用户Socket连接丢失，消息已默认置为Unsent
                    Log.Warn(cSingleMsg.Msg_Content + $"<exe send error,aim socket {cSingleMsg.To_User_ID} lost>", eEvent.Client.LogSource);
                }
            }
            else
            {
                //目标用户离线，消息已默认置为Unsent
                Log.Warn(cSingleMsg.Msg_Content+ $"<exe send error,aim user {cSingleMsg.To_User_ID} is offline>",eEvent.Client.LogSource);
            }

           

            
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
