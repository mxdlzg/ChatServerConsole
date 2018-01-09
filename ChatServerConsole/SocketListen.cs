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

namespace ChatServerConsole
{
    public enum StateEnum
    {
        Receive,
        Send,
        ServerStop,
        Default
    }

    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
        public StateEnum Status = StateEnum.Receive;

        public StateObject(Socket workSocket)
        {
            this.workSocket = workSocket;
        }

        public StateObject()
        {

        }
    }

    public class SendObject:StateObject
    {
        public Event_Type EventType { get; set; }
        public Client Client { get; set; }
        public object RawContent { get; set; }
        public bool Success { get; set; }
    }


    public class SocketListen
    {
        public static ManualResetEvent AllDone = new ManualResetEvent(false);
        public static List<Client> Clients { get; set; }
        private static readonly string[] eofArr =  { "<EOF>" };
        private const string eof = "<EOF>";
        private static bool IsRunning = true;
        public static Socket listener;


        /// <summary>
        /// Find Aim Client
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        public static Client FindClient(Tuple<IPAddress, int> tuple)
        {
            lock (Clients)
            {
                foreach (var client in Clients)
                {
                    if (Equals(client.Address, tuple.Item1) && client.Port.Equals(tuple.Item2))
                    {
                        return client;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// start socket server
        /// </summary>
        public static void StartListening()
        {
            byte[] bytes = new Byte[1024];

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());   //local ip config
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 58888);

            //create socket
            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //list
            Clients = new List<Client>();

            //bind
            try
            {
                listener.Bind(ipEndPoint);
                listener.Listen(100);

                while (IsRunning)
                {
                    AllDone.Reset();

                    Log.Info("waiting for connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    AllDone.WaitOne();  //等待刚才开启的线程返回确认消息，否则主线程阻塞
                }

                //listener.Shutdown(SocketShutdown.Both);
                //listener.Close();
                Log.Info("Stopping Success!");
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            Log.Info("press enter to continue!");
            Console.Read();
        }

        /// <summary>
        /// Stop Server
        /// </summary>
        public static void StopListening()
        {
            Log.Info("Stopping Server...");
            IsRunning = false;
            Log.Info("Stopping Server...Is Running=false");

            Thread.Sleep(10000);
            lock (Clients)
            {
                foreach (Client client in Clients)
                {
                    client.DisConnect();
                }
                Clients.Clear();
            }
            AllDone.Set();
        }

        /// <summary>
        /// accept a new socket
        /// </summary>
        /// <param name="ar"></param>
        private static void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();//告知主线程，继续工作

            Log.Info("");

            //socket
            Socket listener = (Socket)ar.AsyncState;        //at main thread

            //child sock
            Socket handler = listener.EndAccept(ar);        //new child sock

            //remote Info
            IPEndPoint ipEndPoint = ((IPEndPoint)handler.RemoteEndPoint);

            //如果server正在进入停止，拒绝建立socket
            if (!IsRunning)
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                Log.Warn("child coming, refuse child!", ipEndPoint.Address + ":" + ipEndPoint.Port);
                return;
            }
            else
            {
                Log.Info("child coming, accept child!", ipEndPoint.Address + ":" + ipEndPoint.Port);
            }

            //Own entity
            StateObject stateObject = new StateObject(handler);
            Client client = new Client(stateObject);


            //add to list
            lock (Clients)
            {
                Clients.Add(client);
            }

            //
            handler.Send(Encoding.UTF8.GetBytes("received"));

            //begin receive
            handler.BeginReceive(stateObject.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback),client);
        }

        /// <summary>
        /// close this socket & client
        /// </summary>
        /// <param name="client"></param>
        private static void CloseSocket(Client client)
        {
            lock (client)
            {
                if (client.Login)   //如果是登录状态，退出此地址的登录
                {
                    client.Logout();//TODO::Logout
                }
                if (client.Socket != null && client.Socket.Connected)
                {
                    client.Socket.Shutdown(SocketShutdown.Both);
                    client.Socket.Close();
                }
                lock (Clients)
                {
                    Clients.Remove(client);
                }
            }
            Log.Warn("async socket end", client.Address + ":" + client.Port);
        }


        /// <summary>
        /// call after read
        /// </summary>
        /// <param name="ar"></param>
        private static void ReadCallback(IAsyncResult ar)
        {
            if (!IsRunning)
            {
                return;
            }

            String content = String.Empty;

            Client client = (Client) ar.AsyncState;
            Socket handler = client.Socket;
            ClientEvent eEvent;
            Log.Info("start read",client.LogSource);

            int bytesRead = 0;
            try
            {
                //read data
                bytesRead = handler.EndReceive(ar);

            }
            catch (SocketException e)
            {
                //error
                Log.Error(e.ToString());
                //socket error,close this socket
                CloseSocket(client);
            }

            if (bytesRead > 0)
            {
                //append data
                client.State.sb.Append(Encoding.UTF8.GetString(client.State.buffer, 0, bytesRead));

                //check eof end flag
                content = client.State.sb.ToString();


                //check
                int pos = content.LastIndexOf("<EOF>"),count = 0;
                if (pos>-1)
                {
                    //clear sb data
                    client.State.sb.Clear();

                    //if client exit
                    if (content.Equals("exit<EOF>"))
                    {
                        CloseSocket(client);
                        return;
                    }

                    //var
                    string[] arr = content.Split(eofArr,StringSplitOptions.RemoveEmptyEntries);
                    count = arr.Length;

                    //remove last uncompleted data
                    if (pos != content.Length-5)//if has data behind <EOF>
                    {
                        client.State.sb.Append(arr[arr.Length - 1]);
                        count = count-1;
                    }

                    //continue to read
                    handler.BeginReceive(client.State.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), client);

                    Log.Warn(content+"--<read>",client.LogSource);

                    //despatch all Event
                    for (int i = 0; i < count; i++)
                    {
                        eEvent = !arr[i].Equals("")?new ClientEvent(client,arr[i]):null;

                        //despatch all event,and this handler wiil query db and send back to its request in another thread
                        client.EventHandler.DespatchEvent(eEvent);
                    }
                }
                else
                {
                    //get more
                    handler.BeginReceive(client.State.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), client);
                }
            }
        }

        /// <summary>
        /// send
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="content"></param>
        /// <param name="client"></param>
        public static void Send(Socket handler, string content,Client client)
        {
            //bytes
            byte[] bytes = Encoding.UTF8.GetBytes(content+eof);

            //Send
            if (handler.Connected)
            {
                //send async
                handler.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), client);

                //Log
                Log.Info(content + " <" + bytes.Length + ">", client.LogSource);
            }
            else
            {
                //remove client
                CloseSocket(client);

                //Log
                Log.Warn("socket closed by client side",client.LogSource);
            }
            
        }

        public static void Send(Socket handler, string content, SendObject sendObject)
        {
            //bytes
            byte[] bytes = Encoding.UTF8.GetBytes(content + eof);

            //Send
            if (handler.Connected)
            {
                //send async,使用sendObject传递，可以在发送成功之后执行指定的动作
                handler.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendEditCallback), sendObject);

                //Log
                Log.Info(content + " <" + bytes.Length + ">", sendObject.Client.LogSource);
            }
            else
            {
                //remove client
                CloseSocket(sendObject.Client);

                //Log
                Log.Warn("socket closed by client side", sendObject.Client.LogSource);
            }
        }

        private static void SendEditCallback(IAsyncResult ar)
        {
            try
            {
                SendObject sendObject = (SendObject) ar.AsyncState;

                int bytesSent = sendObject.Client.Socket.EndSend(ar);
                if (bytesSent > 0)
                {
                    //handler
                    ServerEventHandler.DespatchEvent(new ServerEvent() {CallbackType = sendObject.EventType,CallbackState = sendObject.RawContent,CallbackSuccess = true});

                    //Log
                    Log.Info("send bytes " + bytesSent, sendObject.Client.LogSource);
                }
                else
                {
                    //handler
                    ServerEventHandler.DespatchEvent(new ServerEvent() { CallbackType = sendObject.EventType, CallbackState = sendObject.RawContent, CallbackSuccess = false });

                    //Log
                    Log.Warn("send fail, bytes " + bytesSent, sendObject.Client.LogSource);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        /// <summary>
        /// call after send
        /// </summary>
        /// <param name="ar"></param>
        public static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Client client = (Client) ar.AsyncState;
                
                int bytesSent = client.Socket.EndSend(ar);

                if (bytesSent>0)
                {
                    //Log
                    Log.Info("send bytes " + bytesSent, client.LogSource);
                }
                else
                {
                    //Log
                    Log.Warn("send fail, bytes " + bytesSent, client.LogSource);
                }
                

                //如果是关闭命令，则关闭此socket
                if (client.State.Status == StateEnum.ServerStop)
                {
                    client.State.Status = StateEnum.Default;
                    CloseSocket(client);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.StackTrace);
            }
        }
    }
}
