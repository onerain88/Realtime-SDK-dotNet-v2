using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud.Realtime {
    public class SynchronizationObject {
        // 处理事件队列
        readonly Queue<Action> actions;
        readonly AutoResetEvent are;

        internal bool Running { get; set; }

        internal SynchronizationObject() {
            actions = new Queue<Action>();
            are = new AutoResetEvent(false);
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
}
