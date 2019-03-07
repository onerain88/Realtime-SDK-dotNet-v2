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
                    HandleMembersJoined(command);
                    break;
                case OpType.MembersLeft:
                    HandleMembersLeft(command);
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
            if (Connection.idToClient.TryGetValue(clientId, out var client)) {
                // ？？？是不是应该把这个接口实现放到 Connection 中
                Connection.QueryConversationAsync(clientId, cid).ContinueWith(t => { 
                    if (t.IsFaulted) {
                        AVRealtime.PrintLog(t.Exception.InnerException.Message);
                    } else {
                        var conv = t.Result;
                        AVRealtime.Context.Post(() => {
                            client.HandleJoinedConversation(conv);
                        });
                    }
                });
            } else { 
                // TODO 没有查找到对应 IM Client

            }
        }

        void HandleMembersJoined(GenericCommand command) {
            var clientId = command.peerId;
            var membersJoined = command.convMessage;
            var cid = membersJoined.Cid;
            var memberIds = membersJoined.M;
            if (Connection.idToClient.TryGetValue(clientId, out var client)) {
                if (client.idToConversation.TryGetValue(cid, out var conversation)) {
                    client.HandleMembersJoined(conversation, memberIds);
                } else {
                    Connection.QueryConversationAsync(clientId, cid).ContinueWith(t => { 
                        if (t.IsFaulted) {
                            AVRealtime.PrintLog(t.Exception.InnerException.Message);
                        } else {
                            var conv = t.Result;
                            AVRealtime.Context.Post(() => {
                                client.HandleMembersJoined(conversation, memberIds);
                            });
                        }
                    });
                }
            } else {
                // TODO 没有查找到对应 IM Client

            }
        }

        void HandleMembersLeft(GenericCommand command) {
            var clientId = command.peerId;
            var membersLeft = command.convMessage;
            var cid = membersLeft.Cid;
            var memberIds = membersLeft.M;
            if (Connection.idToClient.TryGetValue(clientId, out var client)) {
                if (client.idToConversation.TryGetValue(cid, out var conversation)) {
                    client.HandleMemebersLeft(conversation, memberIds);
                } else {
                    Connection.QueryConversationAsync(clientId, cid).ContinueWith(t => {
                        if (t.IsFaulted) {
                            AVRealtime.PrintLog(t.Exception.InnerException.Message);
                        } else {
                            var conv = t.Result;
                            AVRealtime.Context.Post(() => {
                                client.HandleMemebersLeft(conversation, memberIds);
                            });
                        }
                    });
                }
            } else {
                // TODO 没有查找到对应 IM Client

            }
        }
    }
}
