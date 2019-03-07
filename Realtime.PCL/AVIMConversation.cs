using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCloud.Realtime {
    public class AVIMConversation {
        public string convId;
        public Dictionary<string, object> rawData;

        public AVIMClient Client {
            get; internal set;
        }

        public Task<AVIMMessage> SendMessageAsync(AVIMMessage message) {
            return Client.SendMessageAsync(this, message);
        }
    }
}
