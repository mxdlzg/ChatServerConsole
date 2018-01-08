using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ChatServerConsole.Model
{
    public enum Event_Type
    {
        Register,
        Login,
        Logout,
        SendMessage,
        PublishMessage,
        Unknown,
        Bad
    }

    public class ClientEvent
    {
        public Event_Type Type { get; set; }
        public string RawContent { get; set; }
        public dynamic Body { get; set; }
        public string SendTime { get; set; }
        public Client Client { get; set; }
        public dynamic dictionary { get; set; }
        public ClientArgs Args { get; set; }

        public static JavaScriptSerializer script = new JavaScriptSerializer();

        public ClientEvent(Client client,string rawContent)
        {
            this.RawContent = rawContent;
            this.Client = client;
            try
            {
                dictionary = script.DeserializeObject(rawContent) as Dictionary<string,object>;
                this.Type = (Event_Type) (dictionary["type"] != null? Enum.Parse(typeof(Event_Type), dictionary["type"]): Event_Type.Unknown);
                this.Body = dictionary["body"];
                this.SendTime = dictionary["time"] as string;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
