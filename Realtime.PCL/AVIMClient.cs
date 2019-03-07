using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LeanCloud.Realtime {
    public class AVIMClient {
        readonly static Dictionary<string, AVIMClient> clients = new Dictionary<string, AVIMClient>();

        /// <summary>
        /// 被邀请加入到会话
        /// </summary>
        public event Action<AVIMConversation> OnJoinedConversation;
        /// <summary>
        /// 有用户加入到会话
        /// </summary>
        public event Action<AVIMConversation, List<string>> OnMembersJoined;
        /// <summary>
        /// 有用户离开会话
        /// </summary>
        public event Action<AVIMConversation, List<string>> OnMembersLeft;

        /// <summary>
        /// 接收消息事件
        /// </summary>
        public event Action<AVIMMessage> OnReceivedMessage;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event Action OnDisconnected;
        /// <summary>
        /// 重连成功事件
        /// </summary>
        public event Action OnReconnected;

        public string ClientId {
            get; private set;
        }

        Connection connection;

        internal readonly Dictionary<string, AVIMConversation> idToConversation = new Dictionary<string, AVIMConversation>();

        AVIMClient(string clientId) {
            ClientId = clientId;

        }

        /// <summary>
        /// 获取 AVIMClient，如果不存在，则实例化
        /// </summary>
        /// <returns>The client.</returns>
        /// <param name="clientId">Client identifier.</param>
        public static AVIMClient GetInstance(string clientId) {
            // TODO 判断 clientId 合法性
            if (string.IsNullOrEmpty(clientId)) {
                throw new Exception("client id is null");
            }
            lock (clients) {
                if (clients.TryGetValue(clientId, out var client)) {
                    return client;
                }
                var newClient = new AVIMClient(clientId);
                clients.Add(clientId, newClient);
                return newClient;
            }
        }

        /// <summary>
        /// 打开会话
        /// </summary>
        /// <returns>The open.</returns>
        public Task Open() {
            return AVRealtime.GetConnection("Eohx7L4EMfe4xmairXeT7q1w-gzGzoHsz").ContinueWith(t => {
                if (t.IsFaulted) {
                    AVRealtime.PrintLog(t.Exception.InnerException.Message);
                    var tcs = new TaskCompletionSource<bool>();
                    tcs.SetException(t.Exception.InnerException);
                    return tcs.Task;
                }
                // TODO 在 SDK 上下文中设置
                connection = t.Result;
                return connection.OpenSession(this);
            }).Unwrap();
        }

        // TODO 完善参数
        /// <summary>
        /// 创建会话
        /// </summary>
        /// <returns>The conversation async.</returns>
        /// <param name="memberIds">Member identifiers.</param>
        public Task<AVIMConversation> CreateConversationAsync(List<string> memberIds) {
            return connection.CreateConversationAsync(ClientId, memberIds);
        }

        // TODO 完善参数
        /// <summary>
        /// 加入会话
        /// </summary>
        /// <returns>The conversation async.</returns>
        /// <param name="convId">Conv identifier.</param>
        public Task<AVIMConversation> JoinConversationAsync(string convId) {
            return connection.JoinConversationAsync(ClientId, convId);
        }

        /// <summary>
        /// 离开会话
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="convId">Conv identifier.</param>
        public Task QuitAsync(string convId) {
            return connection.QuitConversationAsync(ClientId, convId, new List<string> { convId }).ContinueWith(t => {
                AVRealtime.Context.Post(() => {
                    idToConversation.Remove(convId);
                });
            });
        }

        /// <summary>
        /// 踢出会话
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="convId">Conv identifier.</param>
        /// <param name="memberIdList">Member identifier list.</param>
        public Task KickAsync(string convId, List<string> memberIdList) {
            return connection.QuitConversationAsync(ClientId, convId, memberIdList);
        }

        public Task<AVIMConversation> QueryConversationAsync(string convId) {
            return connection.QueryConversationAsync(ClientId, convId);
        }

        internal Task<AVIMMessage> SendMessageAsync(AVIMConversation conversation, AVIMMessage message) {
            return connection.SendMessageAsync(this, conversation, message);
        }

        internal void HandleDisconnection() {
            OnDisconnected?.Invoke();
        }

        internal void HandleReconnected() {
            connection.ReOpenSession(ClientId).ContinueWith(t => {
                if (t.IsFaulted) {
                    AVRealtime.PrintLog(t.Exception.InnerException.Message);
                    return;
                }
                // IM Client 重连完成
                OnReconnected?.Invoke();
            });
        }

        internal void UpdateConversation(AVIMConversation conv) {
            conv.Client = this;
            idToConversation.Remove(conv.convId);
            idToConversation.Add(conv.convId, conv);
        }

        internal void RemoveConversation(string convId) {
            idToConversation.Remove(convId);
        }

        internal void HandleReceiveMessage(AVIMConversation conversation, AVIMMessage message) {
            message.Conversation = conversation;
            OnReceivedMessage?.Invoke(message);
        }

        internal void HandleJoinedConversation(AVIMConversation conversation) {
            OnJoinedConversation?.Invoke(conversation);
        }

        internal void HandleMembersJoined(AVIMConversation conversation, List<string> memberIds) {
            OnMembersJoined?.Invoke(conversation, memberIds);
        }

        internal void HandleMemebersLeft(AVIMConversation conversation, List<string> memberIds) {
            OnMembersLeft?.Invoke(conversation, memberIds);
        }
    }
}
