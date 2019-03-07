using System;

namespace LeanCloud.Realtime {
    internal class DirectHandler : CommandHandler {
        internal DirectHandler(Connection connection) : base(connection) {
        
        }

        internal override void Handle(GenericCommand command) {
            var recvMsg = command.directMessage;
            // 获取到 IM Client
            var clientId = command.peerId;
            if (Connection.idToClient.TryGetValue(clientId, out var client)) {
                // 再获取到 Conversation
                var convId = recvMsg.Cid;
                if (client.idToConversation.TryGetValue(convId, out var conversation)) {
                    // 再通过 Conversation 和 Message 通知用户
                    client.HandleReceiveMessage(conversation, new AVIMTextMessage { 
                        Text = "fake cached message"
                    });
                } else {
                    Connection.QueryConversationAsync(clientId, convId).ContinueWith(t => { 
                        if (t.IsFaulted) {
                            AVRealtime.PrintLog(t.Exception.InnerException.Message);
                            return;
                        }
                        AVRealtime.Context.Post(() => {
                            client.HandleReceiveMessage(conversation, new AVIMTextMessage {
                                Text = "fake queried message"
                            });
                        });
                    });
                }
            }
        }
    }
}
