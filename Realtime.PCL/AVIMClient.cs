using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace LeanCloud.Realtime {
    public class AVIMClient {
        static RTMWebSocketClient client;

        AVIMClient() {
            
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <returns>The client async.</returns>
        public static Task<AVIMClient> CreateClientAsync() {
            var tcs = new TaskCompletionSource<AVIMClient>();
            // 1. 请求服务端信息
            var router = new RTMRouter();
            router.GetAsync().ContinueWith(t => {
                client = new RTMWebSocketClient();
                // 2. 建立连接
                return client.Open(t.Result);
            }).Unwrap().ContinueWith(t => {
                // 3. 用户登录
                var cmd = new SessionCommand {
                    configBitmap = 1,
                    Ua = "net-universal/1.0.6999.29889",
                    N = null,
                    T = 0,
                    S = null,
                };
                var genericCmd = new GenericCommand { 
                    I = 1,
                    Cmd = CommandType.Session,
                    Op = OpType.Open,
                    peerId = "leancloud",
                    appId = "Eohx7L4EMfe4xmairXeT7q1w-gzGzoHsz",
                    sessionMessage = cmd
                };
                client.Send(genericCmd);
            });
            return tcs.Task;
        }
    }
}
