using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace testserver
{
    class Add : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Console.WriteLine("Connection Open");
            base.OnOpen();
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            var data = e.Data;
            if (TestJson(data))
            {
                var param = JToken.Parse(data);
                if (param["a"] != null && param["b"] != null)
                {
                    var a = param["a"].ToObject<int>();
                    var b = param["b"].ToObject<int>();
                    Send(JsonConvert.SerializeObject(new { code = 200, msg = "result is " + (a + b) }));
                    //Task.Factory.StartNew(() => {
                    //    Task.Delay(10000).Wait();
                    //    Send(JsonConvert.SerializeObject(new { code = 200, msg = "I just to tell you, the connection is different from http, i still alive and could send message to you." }));
                    //});
                }
            }
            else
            {
                Send(JsonConvert.SerializeObject(new { code = 400, msg = "request is not a json string." }));
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("Connection Closed");
            base.OnClose(e);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.Message);
            base.OnError(e);
        }

        private static bool TestJson(string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
