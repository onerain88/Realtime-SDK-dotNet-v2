using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LeanCloud.Realtime {
    public class AVIMTextMessage : AVIMMessage {
        public string Text {
            get; set;
        }

        public AVIMTextMessage() {

        }

        public override string ToString() {
            var dict = new Dictionary<string, string> {
                { "_lctex", Text }
            };
            return JsonConvert.SerializeObject(dict);
        }
    }
}
