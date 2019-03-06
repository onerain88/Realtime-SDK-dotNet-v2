using System;

namespace LeanCloud.Realtime {
    internal class ConversationHandler : CommandHandler {
        internal ConversationHandler(Connection connection) : base(connection) {

        }

        internal override void Handle(GenericCommand command) {
            switch (command.Op) {
                case OpType.Joined:
                    HandleJoinedConversation(command);
                    break;
                case OpType.MembersJoined:
                    break;
                case OpType.MembersBlocked:
                    break;
                default:
                    break;
            }
        }

        void HandleJoinedConversation(GenericCommand command) {
            var clientId = command.peerId;
            var joinedConvCmd = command.convMessage;
            var initBy = joinedConvCmd.initBy;
            var cid = joinedConvCmd.Cid;
            if (Connection.clientIdToClient.TryGetValue(clientId, out var client)) {
                // ？？？是不是应该把这个接口实现放到 Connection 中
                client.QueryConversationAsync(cid).ContinueWith(t => { 
                    if (t.IsFaulted) {
                        AVRealtime.PrintLog(t.Exception.InnerException.Message);
                        return;
                    }
                    // TODO 用户添加会话对象

                    // TODO 通知用户被加入会话

                });
            }
        }
    }
}
