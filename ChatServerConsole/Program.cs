using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using ChatServerConsole.DB;
using ChatServerConsole.Model;
using Console = System.Console;

namespace ChatServerConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            //DbUserAction dbUser = new DbUserAction(new ChatDBEntities());
            //dbUser.Register("16", "2", "2", "2", "002");



            Log.Reset();
            Log.Info("main thread:" + Thread.CurrentThread.ManagedThreadId);

            Thread thread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(10000);
                SocketListen.StopListening();
            }));
            //thread.Start();

            SocketListen.StartListening();

            Console.ReadKey();
        }

    }
}
