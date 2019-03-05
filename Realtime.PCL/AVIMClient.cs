using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LeanCloud.Realtime {
    public class AVIMClient : SynchronizationObject {
        // Client Id 到 App Id 的映射
        readonly static Dictionary<string, string> clientIdToAppId = new Dictionary<string, string>();

        readonly static Dictionary<string, AVIMClient> clients = new Dictionary<string, AVIMClient>();

        public event Action OnDisconnected;
        public event Action OnReconnected;

        readonly string clientId;

        Connection connection;

        AVIMClient(string clientId) {
            this.clientId = clientId;

        }

        /// <summary>
        /// 获取 AVIMClient，如果不存在，则实例化
        /// </summary>
        /// <returns>The client.</returns>
        /// <param name="clientId">Client identifier.</param>
        public static AVIMClient GetInstance(string clientId) { 
            // TODO 判断 clientId 合法性
            if (string.IsNullOrEmpty(clientId)) {
                throw new Exception("client id is null");
            }
            lock (clients) { 
                if (clients.TryGetValue(clientId, out var client)) {
                    return client;
                }
                var newClient = new AVIMClient(clientId);
                clients.Add(clientId, newClient);
                return newClient;
            }
        }

        /// <summary>
        /// 打开会话
        /// </summary>
        /// <returns>The open.</returns>
        public Task Open() {
            var tcs = new TaskCompletionSource<bool>();
            // 1. 获取到连接完成的 Connection
            AVRealtime.GetConnection("Eohx7L4EMfe4xmairXeT7q1w-gzGzoHsz").ContinueWith(t => {
                if (t.IsFaulted) {

                }
                Post(() => {
                    connection = t.Result;
                });
                // 2. 发送 session/open 消息
                var sessionOpen = new SessionCommand {
                    configBitmap = 1,
                    Ua = "net-universal/1.0.6999.29889",
                    N = null,
                    T = 0,
                    S = null,
                };
                var cmd = new GenericCommand {
                    I = 1,
                    Cmd = CommandType.Session,
                    Op = OpType.Open,
                    peerId = "leancloud",
                    appId = "Eohx7L4EMfe4xmairXeT7q1w-gzGzoHsz",
                    sessionMessage = sessionOpen
                };
                return connection.SendRequest(cmd);
            }).Unwrap().ContinueWith(t => {
                // 3. 在收到 session/opened 消息后，Post 到 Client 线程中设置状态
                var cmd = t.Result;
                var sessionOpened = cmd.sessionMessage;
                // TODO 判断会话打开结果

                tcs.SetResult(true);
            });
            return tcs.Task;
        }

        internal Task<GenericCommand> SendRequest(GenericCommand command) {
            return connection.SendRequest(command);
        }

        internal void Disconnect() {
            OnDisconnected?.Invoke();
        }

        internal void Reconnect() {
            var sessionOpen = new SessionCommand();
            var cmd = new GenericCommand();
            SendRequest(cmd).ContinueWith(t => { 
                if (t.IsFaulted) { 
                    // TODO 重新失败

                } else {
                    // 重连成功
                    OnReconnected?.Invoke();
                }
            });
        }
    }
}
