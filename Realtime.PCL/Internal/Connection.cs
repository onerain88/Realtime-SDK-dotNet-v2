using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCloud.Realtime {
    internal class Connection : SynchronizationObject {
        // 缓存的请求任务
        readonly Dictionary<int, TaskCompletionSource<GenericCommand>> requests;

        // WebSocket 连接
        WebSocketClient websocket;

        readonly Dictionary<string, AVIMClient> clientIdToClient = new Dictionary<string, AVIMClient>();

        // 缓存正在连接的任务
        Task<Connection> connectingTask; 

        internal Connection(string appId) {
            requests = new Dictionary<int, TaskCompletionSource<GenericCommand>>();
        }

        internal Task<Connection> Connect() {
            // 发起连接
            var tcs = new TaskCompletionSource<Connection>();
            // 1. 请求服务端信息
            var router = new RTMRouter();
            router.GetAsync().ContinueWith(t => {
                websocket = new WebSocketClient();
                // 2. 建立连接
                return websocket.Open(t.Result);
            }).Unwrap().ContinueWith(t => {
                // 3. 注册 WebSocket 事件
                websocket.OnMessage += WebSocketClient_OnMessage;
                websocket.OnDisconnected += WebSocketClient_OnDisconnected;
                connectingTask = null;
                tcs.SetResult(this);
            });
            connectingTask = tcs.Task;
            return connectingTask;
        }

        internal Task<GenericCommand> SendRequest(GenericCommand cmd) {
            var tcs = new TaskCompletionSource<GenericCommand>();
            requests.Add(cmd.I, tcs);
            websocket.Send(cmd);
            return tcs.Task;
        }

        void WebSocketClient_OnMessage(GenericCommand cmd) {
            if (cmd.ShouldSerializeI()) {
                // 应答消息
                var reqId = cmd.I;
                if (requests.TryGetValue(reqId, out var tcs)) {
                    tcs.SetResult(cmd);
                } else {
                    // 没有缓存的应答

                }
            } else {
                // TODO 通知消息

            }
        }

        void WebSocketClient_OnDisconnected() {
            foreach (var client in clientIdToClient.Values) {
                client.Disconnect();
            }
        }

    }
}
