using System;
using LeanCloud.Realtime;

namespace Test {
    class MainClass {
        public static void Main(string[] args) {
            Console.WriteLine("Hello World!");

            Websockets.Net.WebsocketConnection.Link();

            //var appId = "Eohx7L4EMfe4xmairXeT7q1w-gzGzoHsz";
            //var appKey = "GSBSGpYH9FsRdCss8TGQed0F";

            AVRealtime.LogPrinter = Console.WriteLine;

            AVIMClient.CreateClientAsync();

            Console.ReadKey(true);
        }
    }
}
