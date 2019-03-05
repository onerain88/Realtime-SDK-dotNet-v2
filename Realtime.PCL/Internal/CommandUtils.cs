using System;

namespace LeanCloud.Realtime {
    internal static class CommandUtils {
        // 请求 id
        static int requestId = 0;
        static readonly object requestIdLockObj = new object();

        static int RequestId {
            get {
                lock (requestIdLockObj) {
                    return requestId++;
                }
            }
        }

        internal static GenericCommand NewCommand(CommandType cmd) {
            var command = new GenericCommand { 
                Cmd = cmd,
                peerId = "leancloud",
                appId = "Eohx7L4EMfe4xmairXeT7q1w-gzGzoHsz",
            };
            return command;
        }

        internal static GenericCommand NewCommand(CommandType cmd, OpType op) {
            var command = NewCommand(cmd);
            command.Op = op;
            return command;
        }

        internal static GenericCommand NewRequest(CommandType cmd) {
            var request = NewCommand(cmd);
            request.I = RequestId;
            return request;
        }

        internal static GenericCommand NewRequest(CommandType cmd, OpType op) {
            var request = NewCommand(cmd, op);
            request.I = RequestId;
            return request;
        }
    }
}
