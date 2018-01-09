using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatServerConsole.DB;

namespace ChatServerConsole.Model
{
    public class Client//integrated security=True;
    {
        public Socket Socket { get; set; }
        public ChatDBEntities DB { get; set; }
        public StateObject State { get; set; }
        public ClientEventHandler EventHandler { get; }
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public DbAction DbAction { get; set; }
        public string LogSource { get; set; }
        public bool Login { get; set; }

        /// <summary>
        /// Client 不使用共享dbcontext
        /// </summary>
        /// <param name="stateObject"></param>
        public Client(StateObject stateObject)
        {
            this.Socket = stateObject.workSocket;
            this.State = stateObject;
            this.EventHandler = new ClientEventHandler();
            IPEndPoint ipEndPoint = ((IPEndPoint)Socket.RemoteEndPoint);
            this.Address = ipEndPoint.Address;
            this.Port = ipEndPoint.Port;
            this.LogSource = Address + ":" + Port;
        }

        /// <summary>
        /// Client 使用共享dbContext
        /// </summary>
        /// <param name="stateObject"></param>
        /// <param name="db"></param>
        public Client(StateObject stateObject, ChatDBEntities db)
        {
            this.Socket = stateObject.workSocket;
            this.State = stateObject;
            this.DB = db;
            this.EventHandler = new ClientEventHandler();
            IPEndPoint ipEndPoint = ((IPEndPoint)Socket.RemoteEndPoint);
            this.Address = ipEndPoint.Address;
            this.Port = ipEndPoint.Port;
            this.LogSource = Address + ":" + Port;
        }

        /// <summary>
        /// disconnect this client
        /// </summary>
        public void DisConnect()
        {
            State.Status = StateEnum.ServerStop;
            SocketListen.Send(Socket, "server exit", this);
        }

        /// <summary>
        /// client send message
        /// </summary>
        /// <param name="content"></param>
        public void Send(string content)
        {
            SocketListen.Send(Socket, content, this);
        }

        /// <summary>
        /// callback中有操作的send方法
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sendObject"></param>
        public void Send(string content, SendObject sendObject)
        {
            SocketListen.Send(Socket,content,sendObject);
        }

        public static void SendToAll(List<Tuple<IPAddress, int>> list,string content)
        {
            foreach (var tuple in list)
            {
                var client = SocketListen.FindClient(tuple);
                client?.Send(content);
            }
        }

        public void Logout()
        {
            int result = DbUserAction.LogoutIp(Address.ToString(), Port);
            if (result<=0)
            {
                Log.Warn("logout fail!",LogSource);
            }
            else
            {
                Log.Warn("update address,port to Offline!",LogSource);
                Login = false;
            }
        }
    }
}
