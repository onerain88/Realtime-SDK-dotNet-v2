using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LeanCloud.Realtime {
    internal class Connection {
        // 缓存的请求任务
        readonly Dictionary<int, TaskCompletionSource<GenericCommand>> requests;

        // WebSocket 连接
        WebSocketClient websocket;

        internal readonly Dictionary<string, AVIMClient> clientIdToClient = new Dictionary<string, AVIMClient>();

        // 缓存正在连接的任务
        Task<Connection> connectingTask;

        readonly CommandFactory commandFactory;

        readonly string appId;
        readonly string appKey;

        internal Connection(string appId, string appKey) {
            this.appId = appId;
            this.appKey = appKey;
            requests = new Dictionary<int, TaskCompletionSource<GenericCommand>>();
            commandFactory = new CommandFactory(appId, appKey);
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

        Task<GenericCommand> SendRequest(GenericCommand cmd) {
            var tcs = new TaskCompletionSource<GenericCommand>();
            requests.Add(cmd.I, tcs);
            websocket.Send(cmd);
            return tcs.Task;
        }

        /// <summary>
        /// 打开会话
        /// </summary>
        /// <returns>The session.</returns>
        /// <param name="clientId">Client identifier.</param>
        internal Task OpenSession(string clientId) {
            var tcs = new TaskCompletionSource<bool>();
            var sessionOpen = new SessionCommand {
                configBitmap = 1,
                Ua = "net-universal/1.0.6999.29889",
                N = null,
                T = 0,
                S = null,
            };
            var cmd = commandFactory.NewRequest(appId, CommandType.Session, OpType.Open);
            cmd.sessionMessage = sessionOpen;
            SendRequest(cmd).ContinueWith(t => {
                var res = t.Result;
                var sessionOpened = res.sessionMessage;
                // TODO 判断会话打开结果

                tcs.SetResult(true);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 重新打开会话
        /// </summary>
        /// <returns>The open session.</returns>
        internal Task ReOpenSession(string clientId) {
            var tcs = new TaskCompletionSource<bool>();
            var sessionOpen = new SessionCommand();
            var cmd = commandFactory.NewRequest(clientId, CommandType.Session, OpType.Open);
            cmd.sessionMessage = sessionOpen;
            SendRequest(cmd).ContinueWith(t => {
                var res = t.Result;
                // TODO 判断会话打开结果

                tcs.SetResult(true);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 创建会话
        /// </summary>
        /// <returns>The conversation async.</returns>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="memberIds">Member identifiers.</param>
        internal Task<AVIMConversation> CreateConversationAsync(string clientId, List<string> memberIds) {
            var tcs = new TaskCompletionSource<AVIMConversation>();
            var createConv = new ConvCommand {
                Unique = true,
            };
            createConv.M.AddRange(memberIds);
            var cmd = commandFactory.NewRequest(appId, CommandType.Conv, OpType.Start);
            cmd.convMessage = createConv;
            SendRequest(cmd).ContinueWith(t => {
                if (t.IsFaulted) {
                    tcs.SetException(t.Exception.InnerException);
                    return null;
                }
                var res = t.Result;
                var createdRes = res.convMessage;
                // TODO 查询会话对象
                return QueryConversationAsync(clientId, createdRes.Cid);
            }).Unwrap().ContinueWith(t => {
                tcs.SetResult(t.Result);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 加入会话
        /// </summary>
        /// <returns>The conversation async.</returns>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="convId">Conv identifier.</param>
        internal Task<AVIMConversation> JoinConversationAsync(string clientId, string convId) {
            var tcs = new TaskCompletionSource<AVIMConversation>();
            var joinConv = new ConvCommand { 
                Cid = convId,
            };
            joinConv.M.Add(clientId);
            var cmd = commandFactory.NewRequest(appId, CommandType.Conv, OpType.Add);
            cmd.convMessage = joinConv;
            SendRequest(cmd).ContinueWith(t => { 
                if (t.IsFaulted) {
                    tcs.SetException(t.Exception.InnerException);
                    return null;
                }
                var res = t.Result;
                var joinedRes = res.convMessage;
                return QueryConversationAsync(clientId, joinedRes.Cid);
            }).Unwrap().ContinueWith(t => {
                tcs.SetResult(t.Result);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 查询会话
        /// </summary>
        /// <returns>The conversation async.</returns>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="convId">Conv identifier.</param>
        internal Task<AVIMConversation> QueryConversationAsync(string clientId, string convId) {
            var tcs = new TaskCompletionSource<AVIMConversation>();
            var where = new Dictionary<string, string> {
                { "objectId", convId }
            };
            var queryConv = new ConvCommand {
                Where = new JsonObjectMessage {
                    Data = JsonConvert.SerializeObject(where),
                },
            };
            var cmd = commandFactory.NewRequest(appId, CommandType.Conv, OpType.Query);
            cmd.convMessage = queryConv;
            SendRequest(cmd).ContinueWith(t => {
                if (t.IsFaulted) {
                    tcs.SetException(t.Exception.InnerException);
                    return;
                }
                var res = t.Result;
                var queriedRes = res.convMessage;
                // TODO 实例化 AVIMConversation 对象
                var convs = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(queriedRes.Results.Data);
                var rawData = convs[0];
                var conv = new AVIMConversation {
                    rawData = rawData
                };
                tcs.SetResult(conv);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 退出会话
        /// </summary>
        /// <returns>The conversation async.</returns>
        /// <param name="convId">Conv identifier.</param>
        /// <param name="memberIdList">Member identifier list.</param>
        internal Task QuitConversationAsync(string clientId, string convId, List<string> memberIdList) {
            var tcs = new TaskCompletionSource<bool>();
            if (memberIdList == null) {
                tcs.SetException(new ArgumentNullException());
            } else {
                var quitConv = new ConvCommand {
                    Cid = convId,
                };
                quitConv.M.AddRange(memberIdList);
                var cmd = commandFactory.NewRequest(appId, CommandType.Conv, OpType.Remove);
                cmd.convMessage = quitConv;
                SendRequest(cmd).ContinueWith(t => {
                    if (t.IsFaulted) {
                        tcs.SetException(t.Exception);
                        return;
                    }
                    var res = t.Result;
                    var quitRes = res.convMessage;
                    // TODO 判断是否成功离开


                });
            }
            return tcs.Task;
        }

        internal Task ClearCache() {
            // TODO 清空 router 缓存

            return Task.FromResult(true);
        }

        internal void Disconnect() {
            websocket.Disconnect();
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
                // 通知消息

            }
        }

        void WebSocketClient_OnDisconnected() {
            // 通知客户端处理断线事件
            foreach (var client in clientIdToClient.Values) {
                client.HandleDisconnection();
            }
            // 开始重连
            Reconnect();
        }

        void Reconnect() {
            // 延迟重连
            Task.Delay(5000).ContinueWith(t => {
                return Connect();
            }).ContinueWith(t => {
                if (t.IsFaulted) {
                    // 如果重连失败，则重复重连过程
                    Reconnect();
                    return;
                }
                // 重连成功
                foreach (var client in clientIdToClient.Values) {
                    client.HandleReconnected();
                }
            });
        }
    }
}
