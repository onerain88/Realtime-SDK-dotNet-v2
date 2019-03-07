﻿using System;
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
            client.Open().ContinueWith(t => {
                Console.WriteLine("☎️ {0}", "client open done");
                var memberIds = new List<string> { "x", "y" };
                return client.CreateConversationAsync(memberIds);
            }).Unwrap().ContinueWith(t => {
                Console.WriteLine("☎️ {0}", "conversation create done");
                Console.WriteLine(t.Result.rawData);
            });
            client.OnDisconnected += () => {
                Console.WriteLine("☎️ {0} is disconnected", client.ClientId);
            };
            client.OnReconnected += () => {
                Console.WriteLine("☎️ {0} is reconnected", client.ClientId);
            };
        }

        static void TestReturnNull() {
            Task.Delay(1000).ContinueWith(t => {
                if (true) {
                    return null;
                }
                return Task.Delay(500);
            }).Unwrap().ContinueWith(t => {
                if (t.IsCanceled) {
                    Console.WriteLine("task is canceled");
                    throw new Exception("cancel task");
                }
                return Task.Delay(500);
            }).Unwrap().ContinueWith(t => {
                Console.WriteLine(t.ToString());
            });
        }

        static void TestContext() {

            //Task.Delay(2000).ContinueWith(t => {
            //    context.Post(state => {
            //        Console.WriteLine($"delay 2000 at {Environment.CurrentManagedThreadId}");
            //    }, null);
            //});
            //Task.Delay(3000).ContinueWith(t => {
            //    context.Post(state => {
            //        Console.WriteLine($"delay 3000 at {Environment.CurrentManagedThreadId}");
            //    }, null);
            //});
        }
    }
}
