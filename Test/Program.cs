using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using LeanCloud.Realtime;
using System.Linq;

namespace Test {

    class MainClass {

        public static void Main(string[] args) {
            Console.WriteLine("Hello World!");

            //var client = new Client();
            //client.Start();
            //for (int i = 0; i < 100; i++) {
            //    var are = new AutoResetEvent(false);
            //    Task.Run(() => {
            //        while (true) {
            //            Console.WriteLine($"run at {Environment.CurrentManagedThreadId}");
            //            Thread.Sleep(1000);
            //        }

            //        //client.Post(() => {
            //        //    Console.WriteLine($"post at {Environment.CurrentManagedThreadId}");
            //        //    client.Post(() => {
            //        //        Console.WriteLine($"post inside at {Environment.CurrentManagedThreadId}");
            //        //    });
            //        //});
            //    }); 
            //}
            //Task.Delay(5000).ContinueWith(t => {
            //    client.Post(() => {
            //        client.Stop();
            //        Console.WriteLine($"delay post at {Environment.CurrentManagedThreadId}");
            //    });
            //});

            TestRealtime();

            //TestReturnNull();

            Console.ReadKey(true);
        }

        static void TestRealtime() {
            Websockets.Net.WebsocketConnection.Link();

            //var appId = "Eohx7L4EMfe4xmairXeT7q1w-gzGzoHsz";
            //var appKey = "GSBSGpYH9FsRdCss8TGQed0F";

            AVRealtime.LogPrinter = Console.WriteLine;

            var client = AVIMClient.GetInstance("xxxxxxx");

            client.OnReceivedMessage += (message) => {
                Console.WriteLine("☎️  {0} received message", client.ClientId);
                if (message is AVIMTextMessage) {
                    Console.WriteLine("☎️  {0} received: {1}", client.ClientId, (message as AVIMTextMessage).Text);
                }
            };
            client.OnDisconnected += () => {
                Console.WriteLine("☎️  {0} is disconnected", client.ClientId);
            };
            client.OnReconnected += () => {
                Console.WriteLine("☎️  {0} is reconnected", client.ClientId);
            };

            //client.Open().ContinueWith(t => {
            //    Console.WriteLine("☎️  {0}", "client open done");
            //    var memberIds = new List<string> { "x", "y" };
            //    return client.CreateConversationAsync(memberIds);
            //}).Unwrap().ContinueWith(t => {
            //    Console.WriteLine("☎️  {0}", "conversation create done");
            //    Console.WriteLine(t.Result.rawData);
            //});

            client.Open().ContinueWith(t => {
                Console.WriteLine("☎️  {0}", "client open done");
                var memberIds = new List<string> { "x", "y" };
                return client.CreateConversationAsync(memberIds);
            }).Unwrap().ContinueWith(t => {
                Console.WriteLine("☎️  {0}", "conversation create done");
                Console.WriteLine(t.Result.rawData);
                var conv = t.Result;
                var msg = new AVIMTextMessage {
                    Text = "hello, world",
                };
                return conv.SendMessageAsync(msg);
            }).Unwrap().ContinueWith(t => {
                Console.WriteLine("☎️  {0}", "send message done");
            });
        }

        //static void TestReturnNull() {
        //    Task.Delay(1000).ContinueWith(t => {
        //        if (true) {
        //            return null;
        //        }
        //        // 只是保证编译
        //        return Task.Delay(500);
        //    }).Unwrap().ContinueWith(t => {
        //        if (t.IsCanceled) {
        //            Console.WriteLine("task is canceled");
        //            throw new Exception("cancel task");
        //        }
        //        return Task.Delay(500);
        //    }).Unwrap().ContinueWith(t => {
        //        Console.WriteLine(t.ToString());
        //    });
        //}
    }
}
