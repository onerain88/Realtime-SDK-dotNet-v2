using System;

namespace LeanCloud.Realtime {
    internal class CommandFactory {
        // 请求 id
        volatile int requestId = 0;
        readonly object requestIdLockObj;

        readonly string appId;
        readonly string appKey;

        internal CommandFactory(string appId, string appKey) {
            this.appId = appId;
            this.appKey = appKey;
            requestIdLockObj = new object();
        }

        int RequestId {
            get {
                lock (requestIdLockObj) {
                    return requestId++;
                }
            }
        }

        internal GenericCommand NewCommand(string clientId, CommandType cmd) {
            var command = new GenericCommand { 
                Cmd = cmd,
                peerId = clientId,
                appId = appId,
            };
            return command;
        }

        internal GenericCommand NewCommand(string clientId, CommandType cmd, OpType op) {
            var command = NewCommand(clientId, cmd);
            command.Op = op;
            return command;
        }

        internal GenericCommand NewRequest(string clientId, CommandType cmd) {
            var request = NewCommand(clientId, cmd);
            request.I = RequestId;
            AVRealtime.PrintLog("request I: {0}", request.I);
            return request;
        }

        internal GenericCommand NewRequest(string clientId, CommandType cmd, OpType op) {
            var request = NewCommand(clientId, cmd, op);
            request.I = RequestId;
            AVRealtime.PrintLog("request I op: {0}", request.I);
            return request;
        }
    }
}
