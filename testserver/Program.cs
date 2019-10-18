using System;
using WebSocketSharp.Server;

namespace testserver
{
    class Program
    {
        static void Main(string[] args)
        {
            var wssv = new WebSocketServer(4649);
            wssv.AddWebSocketService<Add>("/add");
            wssv.Start();
            Console.WriteLine("Server starting, press any key to terminate the server.");
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
