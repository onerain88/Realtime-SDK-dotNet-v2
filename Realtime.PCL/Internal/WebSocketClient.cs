using System;
using System.Threading.Tasks;
using Websockets;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace LeanCloud.Realtime {
    internal class WebSocketClient {
        // 接收消息回调
        internal Action<GenericCommand> OnMessage;
        // 断开连接回调
        internal Action OnDisconnected;

        IWebSocketConnection ws;

        internal Task Open(string url, string protocol = null) {
            AVRealtime.PrintLog("websocket open");
            var tcs = new TaskCompletionSource<bool>();
            void onOpen() {
                AVRealtime.PrintLog("websocket opened");
                ws.OnOpened -= onOpen;
                ws.OnClosed -= onClose;
                ws.OnError -= onError;
                ws.OnMessage += OnWebSocketMessage;
                ws.OnClosed += OnWebSocketDisconnected;
                ws.OnError += OnWebSocketError;
                tcs.SetResult(true);
            }
            void onClose() {
                ws.OnOpened -= onOpen;
                ws.OnClosed -= onClose;
                ws.OnError -= onError;
                tcs.SetException(new Exception("websocket closed when open"));
            }
            void onError(string err) {
                ws.OnOpened -= onOpen;
                ws.OnClosed -= onClose;
                ws.OnError -= onError;
                tcs.SetException(new Exception(string.Format("websocket open error: {0}", err)));
            }
            try {
                ws = WebSocketFactory.Create();
            } catch (Exception e) {
                AVRealtime.PrintLog(e.Message);
            }

            ws.OnOpened += onOpen;
            ws.OnClosed += onClose;
            ws.OnError += onError;
            url = string.Format("{0}?subprotocol=lc.proto2base64.3", "wss://rtm51.leancloud.cn");
            AVRealtime.PrintLog("connect: {0}", url);
            ws.Open(url);
            return tcs.Task;
        }

        internal Task Close() {
            var tcs = new TaskCompletionSource<bool>();
            void onClose() {
                ws.OnClosed -= onClose;
                tcs.SetResult(true);
            }
            ws.OnMessage -= OnWebSocketMessage;
            ws.OnClosed -= OnWebSocketDisconnected;
            ws.OnError -= OnWebSocketError;
            ws.OnClosed += onClose;
            ws.Close();
            return tcs.Task;
        }

        internal void Disconnect() {
            ws.Close();
        }

        internal void Send(GenericCommand cmd) {
            AVRealtime.PrintLog("websocket=>{0}", JsonConvert.SerializeObject(cmd));
            using (var ms = new MemoryStream()) {
                ProtoBuf.Serializer.Serialize(ms, cmd);
                ms.Position = 0;
                var msg = Convert.ToBase64String(ms.ToArray());
                ws.Send(msg);
            }
        }

        // WebSocket 事件
        void OnWebSocketMessage(string msg) {
            //AVRealtime.PrintLog("websocket<={0}", msg);
            // TODO 考虑是否要做反序列化？？？

            byte[] byteArray = Convert.FromBase64String(msg);
            var cmd = ProtoBuf.Serializer.Deserialize<GenericCommand>(new MemoryStream(byteArray));
            AVRealtime.PrintLog("websocket<={0}", JsonConvert.SerializeObject(cmd));
            OnMessage?.Invoke(cmd);
        }

        void OnWebSocketDisconnected() {
            AVRealtime.PrintLog("websocket disconnected");
            OnDisconnected?.Invoke();
        }

        void OnWebSocketError(string err) {
            AVRealtime.PrintLog("websocket error: {0}", err);
            ws.Close();
        }
    }
}
