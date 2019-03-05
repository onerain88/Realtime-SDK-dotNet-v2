using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using LeanCloud.Realtime;

namespace Test {

    class TestScheduler : TaskScheduler {
        protected override IEnumerable<Task> GetScheduledTasks() {
            throw new NotImplementedException();
        }

        protected override void QueueTask(Task task) {
            throw new NotImplementedException();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
            throw new NotImplementedException();

        }

        public override int MaximumConcurrencyLevel { 
            get {
                return 1;
            }
        }
    }

    class Client {
        Queue<Action> actions = new Queue<Action>();
        AutoResetEvent are = new AutoResetEvent(false);

        internal bool Running { get; set; }

        internal void Start() {
            Task.Run(() => {
                Running = true;
                while (Running) {
                    if (actions.Count > 0) {
                        Action action = null;
                        lock (actions) {
                            action = actions.Dequeue();
                        }
                        action?.Invoke();
                    } else {
                        are.WaitOne();
                    }
                }
            });
            are.Set();
        }

        internal void Post(Action action) {
            if (action == null)
                return;

            lock (actions) {
                actions.Enqueue(action);
                are.Set();
            }
        }

        internal void Stop() {
            Running = false;
        }
    }

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


            Websockets.Net.WebsocketConnection.Link();

            //var appId = "Eohx7L4EMfe4xmairXeT7q1w-gzGzoHsz";
            //var appKey = "GSBSGpYH9FsRdCss8TGQed0F";

            AVRealtime.LogPrinter = Console.WriteLine;

            var client = AVIMClient.GetInstance("leancloud");
            client.Open().ContinueWith(t => {
                Console.WriteLine("---------------- client open done");
            });

            //ThreadPool.QueueUserWorkItem((state) => { });

            //var tf = new TaskFactory();
            //for (int i = 0; i < 100; i++) {
            //    tf.StartNew(() => {
            //        Console.WriteLine($"start new at {Environment.CurrentManagedThreadId}");
            //    });
            //}
            //TestContext();

            Console.ReadKey(true);
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
