using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ChatServerConsole.Model
{
    public class ServerEvent
    {
        public Event_Type Type { get; set; }
        public string RawContent { get; set; }
        public string SendTime { get; set; }

        public static JavaScriptSerializer script = new JavaScriptSerializer();

        public ServerEvent()
        {

        }

        public ServerEvent(Event_Type type,string cRawContent)
        {
            this.Type = type;
            this.RawContent = cRawContent;
        }

        public override string ToString()
        {
            return script.Serialize(new ServerEvent() {Type = this.Type,RawContent = this.RawContent,SendTime = DateTime.Now.ToString()});
        }
    }
}
