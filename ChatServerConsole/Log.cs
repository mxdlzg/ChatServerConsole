using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServerConsole
{
    public class Log
    {
        public static bool ToFile = false;
        public static LinkedList<string> log;

        private static readonly string InfoFormat = "Info {2} <{1}>:{0}";
        private static readonly string WarnFormat = "Warn {2} <{1}>:{0}";
        private static readonly string ErrorFormat = "Error {2} <{1}>:{0}";

        public static void Reset()
        {
            if (log == null)
            {
                log = new LinkedList<string>();
            }
            else
            {
                log.Clear();
            }
        }

        /// <summary>
        /// 讲积压的日志写入文件，此方法应在单独的线程中调用，异步IO，在屏幕输出的同时日志并未写入文件，除非此函数被调用
        /// </summary>
        public static void OutToFile()
        {
            lock (log)
            {
                //TODO::
            }
        }

        public static void Info(string content,string source="")
        {
            content = string.Format(InfoFormat, content, source,Thread.CurrentThread.ManagedThreadId);
            Out(content,ConsoleColor.White);
        }

        public static void Warn(string content,string source="")
        {
            content = string.Format(WarnFormat, content, source, Thread.CurrentThread.ManagedThreadId);
            Out(content,ConsoleColor.Yellow);
        }

        public static void Error(string content, string source = "")
        {
            content = string.Format(ErrorFormat, content, source, Thread.CurrentThread.ManagedThreadId);
            Out(content,ConsoleColor.Red);
        }

        private static void Out(string content, ConsoleColor color)
        {
            if (ToFile)
            {
                lock (log)
                {
                    log.AddLast(content);
                }
            }

            if (Console.ForegroundColor != color)
            {
                Console.ForegroundColor = color;
            }
            Console.WriteLine(content);
        }
    }
}
