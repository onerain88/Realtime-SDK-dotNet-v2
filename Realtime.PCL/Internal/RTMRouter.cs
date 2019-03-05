using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeanCloud.Realtime {
    internal class RTMRouter {
        internal RTMRouter() {

        }

        internal Task<string> GetAsync() {
            var tcs = new TaskCompletionSource<string>();
            var client = new HttpClient();
            var rtmUrl = "eohx7l4e.rtm.lncld.net";
            var url = string.Format("https://{0}/v1/route?appId={1}&secure=1", rtmUrl, "Eohx7L4EMfe4xmairXeT7q1w-gzGzoHsz");
            client.GetAsync(url).ContinueWith(t => {
                if (t.IsFaulted) {
                    var exception = t.Exception.InnerException;
                    AVRealtime.PrintLog("get router error: {0}", exception.Message);
                    tcs.SetException(exception);
                    return null;
                }
                var content = t.Result.Content;
                return content.ReadAsStringAsync();
            }).Unwrap().ContinueWith(t => {
                AVRealtime.PrintLog("get router: {0}", t.Result);
                var res = JsonConvert.DeserializeObject(t.Result) as JObject;
                if (res.TryGetValue("server", out var serverObj)) {
                    var server = serverObj.ToString();
                    tcs.SetResult(server);
                } else {
                    tcs.SetException(new Exception("no server"));
                }
            });
            return tcs.Task;
        }
    }
}

