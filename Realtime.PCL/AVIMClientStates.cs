using System;
using System.Threading.Tasks;

namespace LeanCloud.Realtime {
    internal class IMClientState {
        protected AVIMClient Client {
            get;
        }

        internal IMClientState(AVIMClient client) {
            Client = client;
        }

        internal virtual Task Open() => throw new NotImplementedException();

        internal virtual Task<object> Handle(string evt, EventArgs args) => throw new NotImplementedException();
    }

    /// <summary>
    /// IM 用户初始状态
    /// </summary>
    internal class IMClientInitState : IMClientState { 
        internal IMClientInitState(AVIMClient client) : base(client) { 
        }

        internal override Task Open() {
            var tcs = new TaskCompletionSource<bool>();
            // TODO 生成 session/open 请求
            var sessionOpen = new SessionCommand {
                configBitmap = 1,
                Ua = "net-universal/1.0.6999.29889",
                N = null,
                T = 0,
                S = null,
            };
            var cmd = CommandUtils.NewRequest(CommandType.Session, OpType.Open);
            cmd.sessionMessage = sessionOpen;
            Client.SendRequest(cmd).ContinueWith(t => {
                if (t.IsFaulted) {
                    tcs.SetException(t.Exception);
                } else {
                    tcs.SetResult(true);
                }
            });
            return tcs.Task;
        }

        internal override Task<object> Handle(string evt, EventArgs args) {
            if (evt == "open") {
                var tcs = new TaskCompletionSource<object>();
                Client.Open().ContinueWith(t => { 
                    if (t.IsFaulted) {
                        tcs.SetException(t.Exception);
                    } else {
                        tcs.SetResult(Client);
                    }
                });
                return tcs.Task;
            }
            return base.Handle(evt, args);
        }
    }

    /// <summary>
    /// IM 用户正在打开状态
    /// </summary>
    internal class IMClientOpeningState : IMClientState { 
        internal IMClientOpeningState(AVIMClient client) : base(client) { 
        }

        internal override Task<object> Handle(string evt, EventArgs args) {
            return base.Handle(evt, args);
        }
    }

    /// <summary>
    /// IM 用户打开完成状态
    /// </summary>
    internal class IMClientOpenedState : IMClientState { 
        internal IMClientOpenedState(AVIMClient client) : base(client) { 
        }

        internal override Task<object> Handle(string evt, EventArgs args) {
            return base.Handle(evt, args);
        }
    }

    /// <summary>
    /// IM 用户断线状态
    /// </summary>
    internal class IMClientDisconnectedState : IMClientState { 
        internal IMClientDisconnectedState(AVIMClient client) : base(client) { 
        }

        internal override Task<object> Handle(string evt, EventArgs args) {
            return base.Handle(evt, args);
        }
    }
}
