using System;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace testserver
{
    class Program
    {
        static void Main(string[] args)
        {
            var wssv = new WebSocketServer(4649);
            wssv.AddWebSocketService<Add>("/add");
            //wssv.AuthenticationSchemes = AuthenticationSchemes.Basic;
            wssv.Realm = "WebSocket Test";
            //wssv.UserCredentialsFinder = id =>
            //{
               // var name = id.Name;
               // return new NetworkCredential(name, "password", "gunfighter");
                // Return user name, password, and roles.
                //return name == "Jason"
                //   ?
                // : null; // If the user credentials aren't found.
            //};
            wssv.Start();
            
            if (wssv.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", wssv.Port);
                foreach (var path in wssv.WebSocketServices.Paths)
                    Console.WriteLine("- {0}", path);
            }
            Console.WriteLine("Server starting, press any key to terminate the server.");
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
