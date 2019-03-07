using System;

namespace LeanCloud.Realtime {
    public class AVIMMessage {
        public AVIMConversation Conversation {
            get; internal set;
        }

        public AVIMMessage() {
        }

        public override string ToString() {
            return base.ToString();
        }
    }
}
