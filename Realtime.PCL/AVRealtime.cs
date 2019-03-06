using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace LeanCloud.Realtime {
    public class AVRealtime {
        readonly static Dictionary<string, Connection> appIdToConnection = new Dictionary<string, Connection>();

        readonly static Dictionary<string, Task<Connection>> appIdToConnectingTask = new Dictionary<string, Task<Connection>>();

        static RTMContext context = null;
        static readonly object contextLockObj = new object();

        /// <summary>
        /// 保证线程安全
        /// </summary>
        /// <value>The context.</value>
        internal static RTMContext Context {
            get { 
                if (context == null) { 
                    lock (contextLockObj) {
                        if (context == null) {
                            context = new RTMContext();
                        }
                    }
                }
                return context;
            }
        }

        internal AVRealtime() {

        }

        /// <summary>
        /// 获取建立完成的连接对象
        /// </summary>
        /// <returns>The connection.</returns>
        /// <param name="appId">App identifier.</param>
        internal static Task<Connection> GetConnection(string appId) {
            // TODO 线程安全问题

            if (appIdToConnection.TryGetValue(appId, out var conn)) {
                return Task.FromResult(conn);
            }
            if (appIdToConnectingTask.TryGetValue(appId, out var task)) {
                return task;
            }
            var tcs = new TaskCompletionSource<Connection>();
            var connection = new Connection(appId, "");
            connection.Connect().ContinueWith(t => { 
                if (t.IsFaulted) {
                    tcs.SetException(t.Exception.InnerException);
                } else {
                    appIdToConnectingTask.Remove(appId);
                    appIdToConnection.Add(appId, t.Result);
                    tcs.SetResult(t.Result);
                }
            });
            appIdToConnectingTask.Add(appId, tcs.Task);
            return tcs.Task;
        }

        #region Log
        public static Action<string> LogPrinter {
            private get; set;
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="log">日志</param>
        public static void PrintLog(string log) {
            // TODO 通过代理输出
            LogPrinter?.Invoke(log);
        }

        public static void PrintLog(string format, params object[] args) {
            string log = string.Format(format, args);
            PrintLog(log);
        }
        #endregion
    }
}
